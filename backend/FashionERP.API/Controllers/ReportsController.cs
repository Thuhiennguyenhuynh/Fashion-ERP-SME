using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using FashionERP.Application.Interfaces;

namespace FashionERP.API.Controllers
{
    [Authorize(Roles = "Admin,Manager,Accountant")]
    public class ReportsController : BaseController
    {
        private readonly IReportService _reportService;
        private readonly ICashTransactionService _cashService;

        // Đã inject thêm ICashTransactionService
        public ReportsController(IReportService reportService, ICashTransactionService cashService)
        {
            _reportService = reportService;
            _cashService = cashService;
        }

        /// <summary>Doanh thu theo khoảng thời gian, groupBy: day|week|month</summary>
        [HttpGet("revenue")]
        public async Task<IActionResult> GetRevenue(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] string groupBy = "month")
        {
            var result = await _reportService.GetRevenueAsync(from, to, groupBy);
            return Ok(result);
        }

        /// <summary>Top sản phẩm bán chạy</summary>
        [HttpGet("top-products")]
        public async Task<IActionResult> GetTopProducts(
            [FromQuery] DateTime from,
            [FromQuery] DateTime to,
            [FromQuery] int top = 10)
        {
            var result = await _reportService.GetTopProductsAsync(from, to, top);
            return Ok(result);
        }

        /// <summary>Giá trị tồn kho</summary>
        [HttpGet("inventory-value")]
        public async Task<IActionResult> GetInventoryValue()
        {
            var result = await _reportService.GetInventoryValueAsync();
            return Ok(result);
        }

        /// <summary>Xuất CSV cho Power BI (reportType: revenue|top-products|inventory-value)</summary>
        [HttpGet("export")]
        public async Task<IActionResult> Export(
            [FromQuery] string reportType,
            [FromQuery] DateTime from,
            [FromQuery] DateTime to)
        {
            var bytes = await _reportService.ExportCsvAsync(reportType, from, to);
            return File(bytes, "text/csv", $"report_{reportType}_{from:yyyyMMdd}.csv");
        }

        // ==========================================
        // CÁC BÁO CÁO TÀI CHÍNH MỚI (ADD-ON TỪ SỔ QUỸ)
        // ==========================================

        /// <summary>Báo cáo Lãi/Lỗ: Revenue - COGS = Gross Profit, kèm margin theo sản phẩm</summary>
        [HttpGet("profit-loss")]
        [Authorize(Roles = "Admin,Manager,Accountant")]
        public async Task<IActionResult> GetProfitLoss([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var result = await _cashService.GetProfitLossAsync(from, to);
            return Ok(result);
        }

        /// <summary>Báo cáo dòng tiền theo ngày/tuần/tháng</summary>
        [HttpGet("cash-flow")]
        [Authorize(Roles = "Admin,Manager,Accountant")]
        public async Task<IActionResult> GetCashFlow(
            [FromQuery] DateTime from, [FromQuery] DateTime to, [FromQuery] string groupBy = "month")
        {
            var result = await _cashService.GetCashFlowAsync(from, to, groupBy);
            return Ok(result);
        }

        /// <summary>Báo cáo chi phí theo danh mục</summary>
        [HttpGet("expenses-by-category")]
        [Authorize(Roles = "Admin,Manager,Accountant")]
        public async Task<IActionResult> GetExpensesByCategory([FromQuery] DateTime from, [FromQuery] DateTime to)
        {
            var result = await _cashService.GetExpensesByCategoryAsync(from, to);
            return Ok(result);
        }
    }
}