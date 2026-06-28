using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FashionERP.Application.DTOs.HR;
using FashionERP.Application.Interfaces;

namespace FashionERP.API.Controllers
{
    [Authorize]
    public class LeavesController : BaseController
    {
        private readonly ILeaveService _leaveService;

        public LeavesController(ILeaveService leaveService)
        {
            _leaveService = leaveService;
        }

        /// <summary>Nhân viên gửi đơn xin nghỉ phép</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLeaveRequestDto request)
        {
            var result = await _leaveService.CreateAsync(request);
            return Created(result, "Gửi đơn nghỉ phép thành công. Chờ quản lý phê duyệt");
        }

        /// <summary>Quản lý duyệt / từ chối đơn nghỉ</summary>
        [HttpPatch("{id:guid}/approve")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Approve(Guid id, [FromBody] ApproveLeaveRequestDto request)
        {
            var result = await _leaveService.ApproveAsync(id, request, CurrentUserId);
            var msg = request.Status == "Approved" ? "Phê duyệt đơn nghỉ phép thành công"
                                                   : "Từ chối đơn nghỉ phép";
            return Ok(result, msg);
        }

        /// <summary>Xem lịch sử nghỉ phép của một nhân viên</summary>
        [HttpGet("employee/{employeeId:guid}")]
        public async Task<IActionResult> GetByEmployee(Guid employeeId)
        {
            var result = await _leaveService.GetByEmployeeAsync(employeeId);
            return Ok(result);
        }

        /// <summary>Danh sách đơn nghỉ chờ duyệt</summary>
        [HttpGet("pending")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> GetPending()
        {
            var result = await _leaveService.GetPendingAsync();
            return Ok(result);
        }
    }
}

