using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FashionERP.Application.DTOs.Procurement;
using FashionERP.Application.Interfaces;

namespace FashionERP.API.Controllers
{
    [Authorize]
    public class PurchaseOrdersController : BaseController
    {
        private readonly IPurchaseOrderService _purchaseOrderService;

        public PurchaseOrdersController(IPurchaseOrderService purchaseOrderService)
        {
            _purchaseOrderService = purchaseOrderService;
        }

        /// <summary>Danh sách phiếu đặt hàng NCC (paged, filter theo status/supplier/khoảng ngày)</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status,
            [FromQuery] Guid? supplierId,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _purchaseOrderService.GetAllAsync(status, supplierId, from, to, page, pageSize);
            return Ok(result);
        }

        /// <summary>Chi tiết 1 phiếu đặt hàng (kèm danh sách items)</summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _purchaseOrderService.GetByIdAsync(id);
            return Ok(result);
        }

        /// <summary>Tạo phiếu đặt hàng mới (trạng thái Draft)</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Warehouse")]
        public async Task<IActionResult> Create([FromBody] CreatePurchaseOrderRequestDto request)
        {
            var result = await _purchaseOrderService.CreateAsync(request, CurrentUserId);
            return Created(result, "Tạo phiếu đặt hàng thành công");
        }

        /// <summary>Xác nhận đặt hàng với NCC: Draft → Ordered</summary>
        [HttpPatch("{id:guid}/confirm")]
        [Authorize(Roles = "Admin,Manager,Warehouse")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            await _purchaseOrderService.ConfirmOrderAsync(id);
            return Ok<object>(null!, "Đã xác nhận đặt hàng với nhà cung cấp");
        }

        /// <summary>Nhận hàng (từng phần hoặc toàn bộ) — tự cộng Inventory + công nợ NCC</summary>
        [HttpPost("{id:guid}/receive")]
        [Authorize(Roles = "Admin,Manager,Warehouse")]
        public async Task<IActionResult> Receive(Guid id, [FromBody] ReceivePurchaseOrderRequestDto request)
        {
            var result = await _purchaseOrderService.ReceiveItemsAsync(id, request, CurrentUserId);
            return Ok(result, "Đã ghi nhận hàng về");
        }

        /// <summary>Ghi nhận thanh toán cho nhà cung cấp — tự giảm công nợ + tạo sổ quỹ chi</summary>
        [HttpPost("{id:guid}/payments")]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Pay(Guid id, [FromBody] PayPurchaseOrderRequestDto request)
        {
            await _purchaseOrderService.PaySupplierAsync(id, request, CurrentUserId);
            return Ok<object>(null!, "Đã ghi nhận thanh toán cho nhà cung cấp");
        }

        /// <summary>Hủy phiếu đặt hàng (chỉ khi chưa nhận hàng)</summary>
        [HttpPatch("{id:guid}/cancel")]
        [Authorize(Roles = "Admin,Manager,Warehouse")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await _purchaseOrderService.CancelAsync(id);
            return Ok<object>(null!, "Đã hủy phiếu đặt hàng");
        }
    }
}
