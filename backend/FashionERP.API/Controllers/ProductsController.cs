using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FashionERP.Application.DTOs.Product;
using FashionERP.Application.Interfaces;

namespace FashionERP.API.Controllers
{
    [Authorize]
    public class ProductsController : BaseController
    {
        private readonly IProductService _productService;
        private readonly ICloudinaryService _cloudinaryService;

        public ProductsController(IProductService productService,
            ICloudinaryService cloudinaryService)
        {
            _productService = productService;
            _cloudinaryService = cloudinaryService;
        }

        /// <summary>Lấy danh sách sản phẩm (có thể lọc theo status, category, keyword)</summary>
        //[HttpGet]
        //[AllowAnonymous]
        //public async Task<IActionResult> GetAll(
        //    [FromQuery] string? status,
        //    [FromQuery] Guid? categoryId,
        //    [FromQuery] string? keyword)
        //{
        //    var result = await _productService.GetAllAsync(status, categoryId, keyword);
        //    return Ok(result);
        //}

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ProductQueryParams p)
    => Ok(await _productService.GetAllAsync(p));

        /// <summary>Lấy chi tiết sản phẩm theo Id</summary>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _productService.GetByIdAsync(id);
            return Ok(result);
        }

        /// <summary>Tạo sản phẩm mới</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([FromBody] CreateProductRequestDto request)
        {
            var result = await _productService.CreateAsync(request);
            return Created(result, "Thêm sản phẩm thành công");
        }

        /// <summary>Cập nhật thông tin sản phẩm</summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequestDto request)
        {
            var result = await _productService.UpdateAsync(id, request);
            return Ok(result, "Cập nhật sản phẩm thành công");
        }

        /// <summary>Xóa sản phẩm</summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _productService.DeleteAsync(id);
            return Ok<object>(null!, "Xóa sản phẩm thành công");
        }

        /// <summary>Upload ảnh chính của sản phẩm</summary>
        [HttpPost("{id:guid}/main-image")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UploadMainImage(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file ảnh");

            var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!Array.Exists(allowed, t => t == file.ContentType.ToLower()))
                return BadRequest("Chỉ chấp nhận file ảnh JPG, PNG hoặc WEBP");

            if (file.Length > 10 * 1024 * 1024)
                return BadRequest("Dung lượng ảnh không được vượt quá 10MB");

            await using var stream = file.OpenReadStream();
            var upload = await _cloudinaryService.UploadImageAsync(
                stream, "fashion-erp/products", $"prod_{id}_main");

            await _productService.UpdateMainImageAsync(id, upload.Url, upload.PublicId);
            return Ok(new { imageUrl = upload.Url }, "Upload ảnh sản phẩm thành công");
        }

        // ─── VARIANTS ─────────────────────────────────────────

        /// <summary>Thêm biến thể mới (size/màu) cho sản phẩm</summary>
        [HttpPost("{id:guid}/variants")]
        [Authorize(Roles = "Admin,Manager,Warehouse")]
        public async Task<IActionResult> AddVariant(
            Guid id, [FromBody] CreateVariantRequestDto request)
        {
            //request = request with { ProductId = id };
            request.ProductId = id;
            var result = await _productService.AddVariantAsync(request);
            return Created(result, "Thêm biến thể sản phẩm thành công");
        }

        /// <summary>Cập nhật biến thể sản phẩm</summary>
        [HttpPut("variants/{variantId:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateVariant(
            Guid variantId, [FromBody] CreateVariantRequestDto request)
        {
            var result = await _productService.UpdateVariantAsync(variantId, request);
            return Ok(result, "Cập nhật biến thể thành công");
        }

        /// <summary>Xóa biến thể sản phẩm</summary>
        [HttpDelete("variants/{variantId:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVariant(Guid variantId)
        {
            await _productService.DeleteVariantAsync(variantId);
            return Ok<object>(null!, "Xóa biến thể thành công");
        }
    }
}

