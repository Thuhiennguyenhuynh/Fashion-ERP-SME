using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FashionERP.Application.DTOs.Procurement;
using FashionERP.Application.Interfaces;

namespace FashionERP.API.Controllers
{
    [Authorize]
    public class SuppliersController : BaseController
    {
        private readonly ISupplierService _supplierService;

        public SuppliersController(ISupplierService supplierService)
        {
            _supplierService = supplierService;
        }

        /// <summary>Danh sách nhà cung cấp (paged, search theo tên/SĐT/email)</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? keyword,
            [FromQuery] bool? isActive,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _supplierService.GetAllAsync(keyword, isActive, page, pageSize);
            return Ok(result);
        }

        /// <summary>Chi tiết 1 nhà cung cấp (kèm công nợ hiện tại)</summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _supplierService.GetByIdAsync(id);
            return Ok(result);
        }

        /// <summary>Tạo nhà cung cấp mới</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Warehouse")]
        public async Task<IActionResult> Create([FromBody] CreateSupplierRequestDto request)
        {
            var result = await _supplierService.CreateAsync(request);
            return Created(result, "Tạo nhà cung cấp thành công");
        }

        /// <summary>Cập nhật thông tin nhà cung cấp</summary>
        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin,Manager,Warehouse")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateSupplierRequestDto request)
        {
            var result = await _supplierService.UpdateAsync(id, request);
            return Ok(result, "Cập nhật nhà cung cấp thành công");
        }

        /// <summary>Bật/tắt trạng thái hoạt động của nhà cung cấp</summary>
        [HttpPatch("{id:guid}/toggle-active")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            await _supplierService.ToggleActiveAsync(id);
            return Ok<object>(null!, "Đã thay đổi trạng thái hoạt động của nhà cung cấp");
        }
    }
}
