using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FashionERP.Application.DTOs.Auth;
using FashionERP.Application.Interfaces;

namespace FashionERP.API.Controllers
{
    [Authorize(Roles = "Admin")]
    public class UsersController : BaseController
    {
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
            => _userService = userService;

        /// <summary>Lấy danh sách tài khoản hệ thống</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _userService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>Admin tạo tài khoản mới cho nhân viên</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateUserRequestDto request)
        {
            var result = await _userService.CreateAsync(request);
            return Created(result, "Tạo tài khoản thành công");
        }

        /// <summary>Admin bật/tắt tài khoản (toggle IsActive)</summary>
        [HttpPatch("{id:guid}/toggle-active")]
        public async Task<IActionResult> ToggleActive(Guid id)
        {
            var result = await _userService.ToggleActiveAsync(id);
            var msg = result.IsActive ? "Kích hoạt tài khoản thành công"
                                         : "Vô hiệu hoá tài khoản thành công";
            return Ok(result, msg);
        }
    }
}