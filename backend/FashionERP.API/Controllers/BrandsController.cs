using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Application.Interfaces;
using FashionERP.Domain.Entities;
using FashionERP.Infrastructure.Data;

namespace FashionERP.API.Controllers
{
    public class BrandsController : BaseController
    {
        private readonly AppDbContext _db;
        private readonly ICloudinaryService _cloudinaryService;

        public BrandsController(AppDbContext db, ICloudinaryService cloudinaryService)
        {
            _db = db;
            _cloudinaryService = cloudinaryService;
        }

        public record CreateBrandDto(string Name, string? Country);

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Brands
                .OrderBy(b => b.Name)
                .Select(b => new { b.Id, b.Name, b.Country, b.LogoUrl })
                .ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([FromBody] CreateBrandDto request)
        {
            if (await _db.Brands.AnyAsync(b => b.Name == request.Name.Trim()))
                throw new DuplicateException($"Thương hiệu '{request.Name}' đã tồn tại");

            var brand = new Brand
            {
                Name = request.Name.Trim(),
                Country = request.Country?.Trim()
            };
            _db.Brands.Add(brand);
            await _db.SaveChangesAsync();
            return Created(new { brand.Id, brand.Name }, "Thêm thương hiệu thành công");
        }

        [HttpPost("{id:guid}/logo")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UploadLogo(Guid id, IFormFile file)
        {
            var brand = await _db.Brands.FindAsync(id)
                ?? throw new NotFoundException("Thương hiệu", id);

            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file ảnh");

            // Xóa logo cũ trên Cloudinary
            if (!string.IsNullOrEmpty(brand.LogoPublicId))
                await _cloudinaryService.DeleteImageAsync(brand.LogoPublicId);

            await using var stream = file.OpenReadStream();
            var upload = await _cloudinaryService.UploadImageAsync(
                stream, "fashion-erp/brands", $"brand_{id}");

            brand.LogoUrl = upload.Url;
            brand.LogoPublicId = upload.PublicId;
            brand.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new { logoUrl = upload.Url }, "Upload logo thành công");
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var brand = await _db.Brands.FindAsync(id)
                ?? throw new NotFoundException("Thương hiệu", id);

            if (await _db.Products.AnyAsync(p => p.BrandId == id))
                throw new BusinessException("Không thể xóa thương hiệu đang có sản phẩm");

            if (!string.IsNullOrEmpty(brand.LogoPublicId))
                await _cloudinaryService.DeleteImageAsync(brand.LogoPublicId);

            _db.Brands.Remove(brand);
            await _db.SaveChangesAsync();
            return Ok<object>(null!, "Xóa thương hiệu thành công");
        }
    }
}


