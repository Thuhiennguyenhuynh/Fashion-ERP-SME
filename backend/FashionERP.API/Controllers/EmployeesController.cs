using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using FashionERP.Application.DTOs.Employee;
using FashionERP.Application.Interfaces;

namespace FashionERP.API.Controllers
{
    [Authorize]
    public class EmployeesController : BaseController
    {
        private readonly IEmployeeService _employeeService;
        private readonly ICloudinaryService _cloudinaryService;

        public EmployeesController(IEmployeeService employeeService,
            ICloudinaryService cloudinaryService)
        {
            _employeeService = employeeService;
            _cloudinaryService = cloudinaryService;
        }

        /// <summary>Lấy danh sách toàn bộ nhân viên</summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _employeeService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>Lấy thông tin chi tiết một nhân viên</summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _employeeService.GetByIdAsync(id);
            return Ok(result);
        }

        /// <summary>Tạo nhân viên mới</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([FromBody] CreateEmployeeRequestDto request)
        {
            var result = await _employeeService.CreateAsync(request);
            return Created(result, "Thêm nhân viên thành công");
        }

        public record UpdateEmployeeStatusDto(string Status);

        /// <summary>Chuyển trạng thái nhân viên (Active/Probation/Resigned)</summary>
        [HttpPatch("{id:guid}/status")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateEmployeeStatusDto request)
        {
            var allowed = new[] { "Active", "Probation", "Resigned" };
            if (!Array.Exists(allowed, s => s == request.Status))
                return BadRequest($"Status không hợp lệ. Chỉ chấp nhận: {string.Join(", ", allowed)}");

            await _employeeService.UpdateStatusAsync(id, request.Status);
            return Ok<object>(null!, $"Cập nhật trạng thái nhân viên thành {request.Status} thành công");
        }

        /// <summary>Cập nhật thông tin nhân viên</summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEmployeeRequestDto request)
        {
            var result = await _employeeService.UpdateAsync(id, request);
            return Ok(result, "Cập nhật nhân viên thành công");
        }

        /// <summary>Xóa nhân viên</summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            await _employeeService.DeleteAsync(id);
            return Ok<object>(null!, "Xóa nhân viên thành công");
        }

        /// <summary>Upload / cập nhật ảnh đại diện nhân viên</summary>
        [HttpPost("{id:guid}/avatar")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> UploadAvatar(Guid id, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("Vui lòng chọn file ảnh");

            var allowed = new[] { "image/jpeg", "image/png", "image/webp" };
            if (!Array.Exists(allowed, t => t == file.ContentType.ToLower()))
                return BadRequest("Chỉ chấp nhận file ảnh JPG, PNG hoặc WEBP");

            if (file.Length > 5 * 1024 * 1024)
                return BadRequest("Dung lượng ảnh không được vượt quá 5MB");

            await using var stream = file.OpenReadStream();
            var upload = await _cloudinaryService.UploadImageAsync(
                stream, "fashion-erp/employees", $"emp_{id}");

            await _employeeService.UpdateAvatarAsync(id, upload.Url, upload.PublicId);
            return Ok(new { avatarUrl = upload.Url }, "Upload ảnh đại diện thành công");
        }
    }
}

