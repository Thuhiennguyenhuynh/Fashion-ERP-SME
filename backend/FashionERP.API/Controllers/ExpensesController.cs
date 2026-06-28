using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Domain.Entities;
using FashionERP.Infrastructure.Data;

namespace FashionERP.API.Controllers
{
    [Authorize(Roles = "Admin,Manager,Accountant")]
    public class ExpensesController : BaseController
    {
        private readonly AppDbContext _db;

        public ExpensesController(AppDbContext db) { _db = db; }

        public record CreateExpenseDto(
            string Category, decimal Amount,
            string? Description, DateTime ExpenseDate);

        /// <summary>Lấy danh sách chi phí theo tháng/năm</summary>
        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] int month = 0, [FromQuery] int year = 0)
        {
            var query = _db.Expenses.AsQueryable();
            if (month > 0) query = query.Where(e => e.ExpenseDate.Month == month);
            if (year > 0) query = query.Where(e => e.ExpenseDate.Year == year);

            var list = await query
                .OrderByDescending(e => e.ExpenseDate)
                .Select(e => new {
                    e.Id,
                    e.Category,
                    e.Amount,
                    e.Description,
                    e.ExpenseDate,
                    e.CreatedAt
                })
                .ToListAsync();

            var total = 0m;
            foreach (var item in list) total += item.Amount;
            return Ok(new { total, items = list });
        }

        /// <summary>Thêm khoản chi phí</summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateExpenseDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Category))
                return BadRequest("Loại chi phí không được để trống");

            if (request.Amount <= 0)
                return BadRequest("Số tiền phải lớn hơn 0");

            var expense = new Expense
            {
                Category = request.Category.Trim(),
                Amount = request.Amount,
                Description = request.Description?.Trim(),
                ExpenseDate = request.ExpenseDate.Date,
                CreatedBy = CurrentUserId
            };
            _db.Expenses.Add(expense);
            await _db.SaveChangesAsync();
            return Created(new { expense.Id, expense.Category, expense.Amount },
                "Thêm chi phí thành công");
        }

        public record UpdateExpenseDto(
    string? Category,
    decimal? Amount,
    string? Description,
    DateTime? ExpenseDate);

        /// <summary>Cập nhật khoản chi phí</summary>
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExpenseDto request)
        {
            var expense = await _db.Expenses.FindAsync(id)
                ?? throw new NotFoundException("Chi phí", id);

            if (request.Amount.HasValue && request.Amount <= 0)
                return BadRequest("Số tiền phải lớn hơn 0");

            if (!string.IsNullOrWhiteSpace(request.Category))
                expense.Category = request.Category.Trim();
            if (request.Amount.HasValue)
                expense.Amount = request.Amount.Value;
            if (!string.IsNullOrWhiteSpace(request.Description))
                expense.Description = request.Description.Trim();
            if (request.ExpenseDate.HasValue)
                expense.ExpenseDate = request.ExpenseDate.Value.Date;

            expense.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(new { expense.Id, expense.Category, expense.Amount }, "Cập nhật chi phí thành công");
        }

        /// <summary>Xóa khoản chi phí</summary>
        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var expense = await _db.Expenses.FindAsync(id)
                ?? throw new NotFoundException("Chi phí", id);
            _db.Expenses.Remove(expense);
            await _db.SaveChangesAsync();
            return Ok<object>(null!, "Xóa chi phí thành công");
        }
    }
}

