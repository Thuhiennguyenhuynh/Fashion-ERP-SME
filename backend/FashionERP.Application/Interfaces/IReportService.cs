using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FashionERP.Application.Interfaces
{
    public record RevenueReportItem(string Period, decimal Revenue, int Orders);
    public record TopProductItem(string ProductName, string Sku, int TotalQty, decimal TotalRevenue);
    public record InventoryValueItem(string ProductName, string Sku, int Quantity, decimal AvgCost, decimal TotalValue);

    public interface IReportService
    {
        /// <summary>Doanh thu theo ngày/tuần/tháng</summary>
        Task<List<RevenueReportItem>> GetRevenueAsync(DateTime from, DateTime to, string groupBy);

        /// <summary>Top sản phẩm bán chạy</summary>
        Task<List<TopProductItem>> GetTopProductsAsync(DateTime from, DateTime to, int top = 10);

        /// <summary>Giá trị tồn kho hiện tại</summary>
        Task<List<InventoryValueItem>> GetInventoryValueAsync();

        /// <summary>Xuất CSV — trả về byte[]</summary>
        Task<byte[]> ExportCsvAsync(string reportType, DateTime from, DateTime to);
    }
}