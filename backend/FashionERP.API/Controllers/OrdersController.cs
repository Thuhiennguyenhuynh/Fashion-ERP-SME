using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FashionERP.Application.DTOs.Order;
using FashionERP.Application.Interfaces;

namespace FashionERP.API.Controllers
{
    [Authorize]
    public class OrdersController : BaseController
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }

        /// <summary>Danh sách đơn hàng — filter + phân trang</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? status,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] Guid? staffId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _orderService.GetAllAsync(status, from, to, staffId, page, pageSize);
            return Ok(result);
        }

        /// <summary>Chi tiết đơn hàng</summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _orderService.GetByIdAsync(id);
            return Ok(result);
        }

        /// <summary>Tạo đơn hàng mới (POS bán hàng)</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager,Sales")]
        public async Task<IActionResult> Create([FromBody] CreateOrderRequestDto request)
        {
            var staffId = CurrentEmployeeId
                ?? throw new Application.Common.BusinessException(
                    "Tài khoản này chưa được liên kết với nhân viên, không thể tạo đơn hàng");

            var result = await _orderService.CreateAsync(request, staffId);
            return Created(result, "Tạo đơn hàng thành công");
        }

        /// <summary>Hoàn tất đơn hàng (Pending → Completed)</summary>
        [HttpPatch("{id:guid}/complete")]
        [Authorize(Roles = "Admin,Manager,Sales")]
        public async Task<IActionResult> Complete(Guid id)
        {
            await _orderService.CompleteAsync(id);
            return Ok<object>(null!, "Hoàn tất đơn hàng thành công");
        }

        /// <summary>Hủy đơn hàng (hoàn tồn kho tự động)</summary>
        [HttpPatch("{id:guid}/cancel")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Cancel(Guid id)
        {
            await _orderService.CancelAsync(id);
            return Ok<object>(null!, "Hủy đơn hàng thành công");
        }

        /// <summary>Tạo phiếu đổi trả hàng</summary>
        [HttpPost("{id:guid}/returns")]
        [Authorize(Roles = "Admin,Manager,Sales")]
        public async Task<IActionResult> CreateReturn(
            Guid id, [FromBody] CreateReturnRequestDto request)
        {
            request.OrderId = id;
            var result = await _orderService.CreateReturnAsync(request, CurrentUserId);
            return Created(result, "Tạo phiếu đổi trả thành công");
        }
    }
}