using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FashionERP.Application.DTOs.HR;
using FashionERP.Application.Interfaces;

namespace FashionERP.API.Controllers
{
    [Authorize]
    public class AttendancesController : BaseController
    {
        private readonly IAttendanceService _attendanceService;

        public AttendancesController(IAttendanceService attendanceService)
        {
            _attendanceService = attendanceService;
        }

        /// <summary>Nhân viên tự check-in (dùng khuôn mặt/QR - gọi API này)</summary>
        [HttpPost("check-in")]
        public async Task<IActionResult> CheckIn([FromBody] CheckInRequestDto request)
        {
            var result = await _attendanceService.CheckInAsync(request);
            return Ok(result, "Check-in thành công");
        }

        /// <summary>Nhân viên tự check-out</summary>
        [HttpPost("check-out")]
        public async Task<IActionResult> CheckOut([FromBody] CheckOutRequestDto request)
        {
            var result = await _attendanceService.CheckOutAsync(request);
            return Ok(result, "Check-out thành công");
        }

        /// <summary>Quản lý tạo chấm công thủ công</summary>
        [HttpPost("manual")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> CreateManual([FromBody] CreateAttendanceManualDto request)
        {
            var result = await _attendanceService.CreateManualAsync(request);
            return Created(result, "Tạo chấm công thủ công thành công");
        }

        /// <summary>Xem bảng chấm công của nhân viên theo tháng/năm</summary>
        [HttpGet("employee/{employeeId:guid}")]
        public async Task<IActionResult> GetByEmployee(
            Guid employeeId,
            [FromQuery] int month = 0,
            [FromQuery] int year = 0)
        {
            if (month == 0) month = DateTime.UtcNow.Month;
            if (year == 0) year = DateTime.UtcNow.Year;

            if (month < 1 || month > 12)
                return BadRequest("Tháng phải từ 1 đến 12");

            if (year < 2000 || year > 2100)
                return BadRequest("Năm không hợp lệ");

            var result = await _attendanceService.GetByEmployeeAsync(employeeId, month, year);
            return Ok(result);
        }
    }
}

