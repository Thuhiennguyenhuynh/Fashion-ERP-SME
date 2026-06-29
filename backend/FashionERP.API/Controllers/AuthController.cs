using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FashionERP.Application.DTOs.Auth;
using FashionERP.Application.Interfaces;

namespace FashionERP.API.Controllers
{
    public class AuthController : BaseController
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>Đăng nhập - trả về access token + refresh token</summary>
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var result = await _authService.LoginAsync(request);
            return Ok(result, "Đăng nhập thành công");
        }

        /// <summary>Làm mới access token bằng refresh token</summary>
        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokenAsync(request);
            return Ok(result, "Làm mới token thành công");
        }

        /// <summary>Đổi mật khẩu (cần đăng nhập)</summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            await _authService.ChangePasswordAsync(CurrentUserId, request);
            return Ok<object>(null!, "Đổi mật khẩu thành công. Vui lòng đăng nhập lại");
        }

        /// <summary>Tạo tài khoản user mới (chỉ Admin)</summary>
        [HttpPost("users")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequestDto request)
        {
            var result = await _authService.CreateUserAsync(request);
            return Created(result, "Tạo tài khoản thành công");
        }

        /// <summary>Vô hiệu hóa tài khoản (chỉ Admin)</summary>
        [HttpDelete("users/{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateUser(Guid id)
        {
            await _authService.DeactivateUserAsync(id);
            return Ok<object>(null!, "Vô hiệu hóa tài khoản thành công");
        }

        /// <summary>Lấy thông tin người dùng hiện tại</summary>
        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            return Ok(new
            {
                UserId = CurrentUserId,
                EmployeeId = CurrentEmployeeId,
                Role = CurrentRole
            });
        }
        /// <summary>Đăng xuất — revoke refresh token phía server</summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync(CurrentUserId);
            return Ok<object>(null!, "Đăng xuất thành công");
        }
    }
}


