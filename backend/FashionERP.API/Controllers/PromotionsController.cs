using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FashionERP.Application.DTOs.Promotion;
using FashionERP.Application.Interfaces;

namespace FashionERP.API.Controllers
{
    [Authorize]
    public class PromotionsController : BaseController
    {
        private readonly IPromotionService _promotionService;

        public PromotionsController(IPromotionService promotionService)
        {
            _promotionService = promotionService;
        }

        /// <summary>Danh sách tất cả chương trình khuyến mãi</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _promotionService.GetAllAsync();
            return Ok(result);
        }

        /// <summary>Tạo chương trình khuyến mãi mới</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create([FromBody] CreatePromotionRequestDto request)
        {
            var result = await _promotionService.CreateAsync(request);
            return Created(result, "Tạo chương trình khuyến mãi thành công");
        }

        /// <summary>Vô hiệu hóa chương trình khuyến mãi</summary>
        [HttpPatch("{id:guid}/deactivate")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Deactivate(Guid id)
        {
            await _promotionService.DeactivateAsync(id);
            return Ok<object>(null!, "Vô hiệu hóa khuyến mãi thành công");
        }

        /// <summary>Kiểm tra và tính giảm giá cho mã voucher (dùng tại POS)</summary>
        [HttpPost("apply")]
        [AllowAnonymous]
        public async Task<IActionResult> Apply([FromBody] ApplyPromotionRequestDto request)
        {
            var result = await _promotionService.ApplyCodeAsync(request);
            return Ok(result);
        }
    }
}


