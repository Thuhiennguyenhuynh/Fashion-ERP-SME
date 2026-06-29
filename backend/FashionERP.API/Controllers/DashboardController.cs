using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Interfaces;
using FashionERP.Domain.Enums;
using FashionERP.Infrastructure.Data;

namespace FashionERP.API.Controllers
{
    [Authorize(Roles = "Admin,Manager,Accountant")]
    public class DashboardController : BaseController
    {
        private readonly AppDbContext _db;
        private readonly ICashTransactionService _cashService;

        public DashboardController(AppDbContext db, ICashTransactionService cashService)
        {
            _db = db;
            _cashService = cashService;
        }

        /// <summary>Thống kê tổng quan: doanh thu, đơn hàng, lãi gộp (COGS thật), quỹ, công nợ NCC</summary>
        [HttpGet("summary")]
        public async Task<IActionResult> Summary([FromQuery] int month = 0, [FromQuery] int year = 0)
        {
            if (month == 0) month = DateTime.UtcNow.Month;
            if (year == 0) year = DateTime.UtcNow.Year;

            var from = new DateTime(year, month, 1);
            var to = from.AddMonths(1).AddTicks(-1);

            // Doanh thu + lãi gộp dùng ĐÚNG 1 nguồn duy nhất: CashTransactionService
            // (khớp với /api/reports/profit-loss, không tính riêng lẻ ở đây nữa)
            var pnl = await _cashService.GetProfitLossAsync(from, to);

            // Số đơn hàng tháng
            var orderCount = await _db.Orders
                .CountAsync(o => o.CreatedAt.Month == month && o.CreatedAt.Year == year);

            // Số dư quỹ hiện tại (real-time, không giới hạn theo tháng)
            var cashBalance = await _cashService.GetCurrentBalanceAsync();

            // Tổng công nợ phải trả NCC (toàn bộ, không giới hạn theo tháng)
            var totalAccountsPayable = await _db.Suppliers.SumAsync(s => (decimal?)s.TotalDebt) ?? 0;

            // Số PO đang chờ xử lý (Draft/Ordered/PartialReceived)
            var pendingPoCount = await _db.PurchaseOrders.CountAsync(p =>
                p.Status == PurchaseOrderStatus.Draft ||
                p.Status == PurchaseOrderStatus.Ordered ||
                p.Status == PurchaseOrderStatus.PartialReceived);

            // Tồn kho thấp
            var lowStockCount = await _db.Inventories.CountAsync(i => i.Quantity <= i.MinStock);

            // Số khách hàng mới tháng
            var newCustomers = await _db.Customers
                .CountAsync(c => c.CreatedAt.Month == month && c.CreatedAt.Year == year);

            // Doanh thu 7 ngày gần nhất
            var today = DateTime.UtcNow.Date;
            var last7Days = await _db.Orders
                .Where(o => o.Status == OrderStatus.Completed && o.CreatedAt.Date >= today.AddDays(-6))
                .GroupBy(o => o.CreatedAt.Date)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(o => o.FinalAmount) })
                .OrderBy(x => x.Date)
                .ToListAsync();

            return Ok(new
            {
                month,
                year,
                revenue = pnl.Revenue,
                cogs = pnl.Cogs,
                grossProfit = pnl.GrossProfit,
                grossMarginPercent = pnl.GrossMarginPercent,
                orderCount,
                cashBalance = cashBalance.CurrentBalance,
                totalAccountsPayable,
                pendingPoCount,
                lowStockCount,
                newCustomers,
                topProducts = pnl.ProfitByProduct.Take(5),
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
