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
        public ReportsController(IReportService reportService)
            => _reportService = reportService;

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
    }
}