using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FashionERP.Application.DTOs.Finance;
using FashionERP.Application.Interfaces;

namespace FashionERP.API.Controllers
{
    [Authorize]
    public class CashTransactionsController : BaseController
    {
        private readonly ICashTransactionService _cashService;

        public CashTransactionsController(ICashTransactionService cashService)
        {
            _cashService = cashService;
        }

        /// <summary>Sổ quỹ thu/chi (paged, filter theo type/category/khoảng ngày)</summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Manager,Accountant")]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? type,
            [FromQuery] string? category,
            [FromQuery] DateTime? from,
            [FromQuery] DateTime? to,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _cashService.GetAllAsync(type, category, from, to, page, pageSize);
            return Ok(result);
        }

        /// <summary>Số dư quỹ hiện tại</summary>
        [HttpGet("balance")]
        [Authorize(Roles = "Admin,Manager,Accountant")]
        public async Task<IActionResult> GetBalance()
        {
            var result = await _cashService.GetCurrentBalanceAsync();
            return Ok(result);
        }

        /// <summary>Ghi thu/chi thủ công (vd: thu tiền mặt bàn giao, chi mặt bằng)</summary>
        [HttpPost]
        [Authorize(Roles = "Admin,Accountant")]
        public async Task<IActionResult> Create([FromBody] CreateCashTransactionRequestDto request)
        {
            var result = await _cashService.CreateManualAsync(request, CurrentUserId);
            return Created(result, "Đã ghi nhận giao dịch sổ quỹ");
        }
    }
}
