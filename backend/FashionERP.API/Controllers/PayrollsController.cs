using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FashionERP.Application.DTOs.HR;
using FashionERP.Application.Interfaces;

namespace FashionERP.API.Controllers
{
    [Authorize(Roles = "Admin,Manager,Accountant")]
    public class PayrollsController : BaseController
    {
        private readonly IPayrollService _payrollService;

        public PayrollsController(IPayrollService payrollService)
        {
            _payrollService = payrollService;
        }

        /// <summary>Lấy bảng lương theo tháng/năm</summary>
        [HttpGet]
        public async Task<IActionResult> GetByMonthYear(
            [FromQuery] int month = 0,
            [FromQuery] int year = 0)
        {
            if (month == 0) month = DateTime.UtcNow.Month;
            if (year == 0) year = DateTime.UtcNow.Year;

            if (month < 1 || month > 12) return BadRequest("Tháng phải từ 1 đến 12");
            if (year < 2000 || year > 2100) return BadRequest("Năm không hợp lệ");

            var result = await _payrollService.GetByMonthYearAsync(month, year);
            return Ok(result);
        }

        /// <summary>Tính lương cho một nhân viên</summary>
        [HttpPost("generate")]
        public async Task<IActionResult> Generate([FromBody] GeneratePayrollRequestDto request)
        {
            var result = await _payrollService.GenerateAsync(request);
            return Created(result, "Tính lương thành công");
        }

        /// <summary>Xác nhận bảng lương (Draft → Confirmed)</summary>
        [HttpPatch("{id:guid}/confirm")]
        public async Task<IActionResult> Confirm(Guid id)
        {
            await _payrollService.ConfirmAsync(id);
            return Ok<object>(null!, "Xác nhận bảng lương thành công");
        }

        /// <summary>Đánh dấu đã trả lương (Confirmed → Paid)</summary>
        [HttpPatch("{id:guid}/mark-paid")]
        public async Task<IActionResult> MarkPaid(Guid id)
        {
            await _payrollService.MarkAsPaidAsync(id);
            return Ok<object>(null!, "Đánh dấu đã trả lương thành công");
        }
        /// <summary>Nhân viên xem lương của mình theo tháng/năm (self-access)</summary>
        [HttpGet("{employeeId:guid}/{year:int}/{month:int}")]
        [Authorize] // Mọi role đều xem được, nhưng chỉ xem của mình
        public async Task<IActionResult> GetByEmployee(Guid employeeId, int year, int month)
        {
            // Staff chỉ xem lương của chính mình; Admin/Manager xem được tất cả
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (role == "Sales" || role == "Warehouse" || role == "Accountant")
            {
                if (CurrentEmployeeId != employeeId)
                    return Forbid();
            }

            if (month < 1 || month > 12) return BadRequest("Tháng không hợp lệ");
            if (year < 2000 || year > 2100) return BadRequest("Năm không hợp lệ");

            var result = await _payrollService.GetByEmployeeMonthAsync(employeeId, year, month);
            if (result == null)
                return NotFound("Chưa có bảng lương cho tháng này");

            return Ok(result);
        }

    }
}
