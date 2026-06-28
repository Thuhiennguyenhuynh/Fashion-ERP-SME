using System;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using FashionERP.Application.Common;

namespace FashionERP.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        /// <summary>Lấy UserId từ JWT claim</summary>
        protected Guid CurrentUserId =>
            Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new ForbiddenException("Không xác định được người dùng hiện tại"));

        /// <summary>Lấy EmployeeId từ JWT claim (nullable)</summary>
        protected Guid? CurrentEmployeeId
        {
            get
            {
                var val = User.FindFirstValue("employeeId");
                return val != null ? Guid.Parse(val) : null;
            }
        }

        /// <summary>Lấy Role từ JWT claim</summary>
        protected string CurrentRole =>
            User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        protected IActionResult Ok<T>(T data, string message = "Thành công") =>
            base.Ok(ApiResponse<T>.Ok(data, message));

        protected IActionResult Created<T>(T data, string message = "Tạo mới thành công") =>
            StatusCode(201, ApiResponse<T>.Ok(data, message));
    }
}

