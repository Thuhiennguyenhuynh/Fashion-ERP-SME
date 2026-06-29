using FashionERP.Application.DTOs.Customer;
using FashionERP.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FashionERP.API.Controllers
{
    [Authorize]
    public class CustomersController : BaseController
    {
        private readonly ICustomerService _customerService;
        private readonly ICloudinaryService _cloudinaryService;

        public CustomersController(ICustomerService customerService,
            ICloudinaryService cloudinaryService)
        {
            _customerService = customerService;
            _cloudinaryService = cloudinaryService;
        }

        /// <summary>Danh sách khách hàng (paged + search + filter)</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] CustomerQueryParams p)
        {
            var result = await _customerService.GetAllAsync(p);
            return Ok(result);
        }

        /// <summary>Chi tiết khách hàng</summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _customerService.GetByIdAsync(id);
            return Ok(result);
        }

        /// <summary>Danh sách đơn hàng của khách (paged)</summary>
        [HttpGet("{id:guid}/orders")]
        public async Task<IActionResult> GetOrders(
            Guid id,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _customerService.GetOrdersByCustomerAsync(id, page, pageSize);
            return Ok(result);
        }

        /// <summary>Tạo khách hàng mới</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCustomerRequestDto request)
        {
            var result = await _customerService.CreateAsync(request);
            return Created(result, "Thêm khách hàng thành công");
        }

        /// <summary>Cập nhật thông tin khách hàng</summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCustomerRequestDto request)
        {
            var result = await _customerService.UpdateAsync(id, request);
            return Ok(result, "Cập nhật khách hàng thành công");
        }

        /// <summary>Xóa mềm khách hàng</summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _customerService.DeleteAsync(id);
            return Ok<object>(null!, "Xóa khách hàng thành công");
        }

        /// <summary>Lưu số đo cơ thể khách hàng</summary>
        [HttpPost("{id:guid}/measurements")]
        public async Task<IActionResult> SaveMeasurement(
            Guid id, [FromBody] CustomerMeasurementDto dto)
        {
            await _customerService.SaveMeasurementAsync(id, dto);
            return Ok<object>(null!, "Lưu số đo thành công");
        }

        /// <summary>Upload ảnh đại diện khách hàng</summary>
        [HttpPost("{id:guid}/avatar")]
        public async Task<IActionResult> UploadAvatar(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file ảnh");
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Dung lượng ảnh không được vượt quá 5MB");

            await using var stream = file.OpenReadStream();
            var upload = await _cloudinaryService.UploadImageAsync(
                stream, "fashion-erp/customers", $"cust_{id}");
            return Ok(new { avatarUrl = upload.Url }, "Upload ảnh thành công");
        }
    }
}