using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionERP.Domain.Enums;
using FashionERP.Infrastructure.Data;

namespace FashionERP.API.Controllers
{
    [Authorize(Roles = "Admin,Manager,Accountant")]
    public class DashboardController : BaseController
    {
        private readonly AppDbContext _db;

        public DashboardController(AppDbContext db) { _db = db; }

        /// <summary>Thống kê tổng quan: doanh thu, đơn hàng, tồn kho thấp, top sản phẩm</summary>
        [HttpGet("summary")]
        public async Task<IActionResult> Summary([FromQuery] int month = 0, [FromQuery] int year = 0)
        {
            if (month == 0) month = DateTime.UtcNow.Month;
            if (year == 0) year = DateTime.UtcNow.Year;

            // Doanh thu tháng
            var revenue = await _db.Orders
                .Where(o => o.Status == OrderStatus.Completed &&
                            o.CreatedAt.Month == month && o.CreatedAt.Year == year)
                .SumAsync(o => (decimal?)o.FinalAmount) ?? 0;

            // Số đơn hàng tháng
            var orderCount = await _db.Orders
                .CountAsync(o => o.CreatedAt.Month == month && o.CreatedAt.Year == year);

            // Chi phí tháng
            var expenses = await _db.Expenses
                .Where(e => e.ExpenseDate.Month == month && e.ExpenseDate.Year == year)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            // Lợi nhuận gộp (chưa trừ thuế, chỉ ước tính)
            var grossProfit = revenue - expenses;

            // Tồn kho thấp
            var lowStockCount = await _db.Inventories
                .CountAsync(i => i.Quantity <= i.MinStock);

            // Số khách hàng mới tháng
            var newCustomers = await _db.Customers
                .CountAsync(c => c.CreatedAt.Month == month && c.CreatedAt.Year == year);

            // Top 5 sản phẩm bán chạy tháng
            var topProducts = await _db.OrderItems
                .Where(oi => oi.Order.Status == OrderStatus.Completed &&
                             oi.Order.CreatedAt.Month == month &&
                             oi.Order.CreatedAt.Year == year)
                .GroupBy(oi => new { oi.ProductName })
                .Select(g => new
                {
                    ProductName = g.Key.ProductName,
                    TotalQty = g.Sum(x => x.Quantity),
                    TotalRevenue = g.Sum(x => x.LineTotal)
                })
                .OrderByDescending(x => x.TotalQty)
                .Take(5)
                .ToListAsync();

            // Doanh thu 7 ngày gần nhất
            var today = DateTime.UtcNow.Date;
            var last7Days = await _db.Orders
                .Where(o => o.Status == OrderStatus.Completed &&
                            o.CreatedAt.Date >= today.AddDays(-6))
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(o => o.FinalAmount) })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Ok(new
            {
                month,
                year,
                revenue,
                orderCount,
                expenses,
                grossProfit,
                lowStockCount,
                newCustomers,
                topProducts,
                last7Days
            });
        }

        /// <summary>Thống kê doanh thu theo tháng trong năm</summary>
        [HttpGet("revenue-by-month")]
        public async Task<IActionResult> RevenueByMonth([FromQuery] int year = 0)
        {
            if (year == 0) year = DateTime.UtcNow.Year;

            var data = await _db.Orders
                .Where(o => o.Status == OrderStatus.Completed && o.CreatedAt.Year == year)
                .GroupBy(o => o.CreatedAt.Month)
                .Select(g => new { Month = g.Key, Revenue = g.Sum(o => o.FinalAmount) })
                .OrderBy(x => x.Month)
                .ToListAsync();

            return Ok(data);
        }

        /// <summary>Thống kê phương thức thanh toán</summary>
        [HttpGet("payment-methods")]
        public async Task<IActionResult> PaymentMethods(
            [FromQuery] int month = 0, [FromQuery] int year = 0)
        {
            if (month == 0) month = DateTime.UtcNow.Month;
            if (year == 0) year = DateTime.UtcNow.Year;

            var data = await _db.Orders
                .Where(o => o.Status == OrderStatus.Completed &&
                            o.CreatedAt.Month == month && o.CreatedAt.Year == year)
                .GroupBy(o => o.PaymentMethod)
                .Select(g => new
                {
                    Method = g.Key.ToString(),
                    Count = g.Count(),
                    Total = g.Sum(o => o.FinalAmount)
                })
                .ToListAsync();

            return Ok(data);
        }
    }
}


