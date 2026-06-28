using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Interfaces;
using FashionERP.Infrastructure.Data;

namespace FashionERP.Infrastructure.Services
{
    public class ReportService : IReportService
    {
        private readonly AppDbContext _db;
        public ReportService(AppDbContext db) => _db = db;

        public async Task<List<RevenueReportItem>> GetRevenueAsync(
            DateTime from, DateTime to, string groupBy)
        {
            var orders = await _db.Orders
                .Where(o => o.Status == "Completed"
                         && o.CompletedAt >= from
                         && o.CompletedAt <= to)
                .Select(o => new { o.CompletedAt, o.FinalAmount })
                .ToListAsync();

            IEnumerable<RevenueReportItem> result = groupBy.ToLower() switch
            {
                "day" => orders
                    .GroupBy(o => o.CompletedAt!.Value.Date.ToString("yyyy-MM-dd"))
                    .Select(g => new RevenueReportItem(g.Key, g.Sum(x => x.FinalAmount), g.Count())),
                "week" => orders
                    .GroupBy(o => ISOWeek.GetYear(o.CompletedAt!.Value) + "-W"
                                + ISOWeek.GetWeekOfYear(o.CompletedAt!.Value).ToString("D2"))
                    .Select(g => new RevenueReportItem(g.Key, g.Sum(x => x.FinalAmount), g.Count())),
                _ => orders // default: month
                    .GroupBy(o => o.CompletedAt!.Value.ToString("yyyy-MM"))
                    .Select(g => new RevenueReportItem(g.Key, g.Sum(x => x.FinalAmount), g.Count()))
            };

            return result.OrderBy(r => r.Period).ToList();
        }

        public async Task<List<TopProductItem>> GetTopProductsAsync(
            DateTime from, DateTime to, int top = 10)
        {
            return await _db.OrderItems
                .Include(oi => oi.Order)
                .Where(oi => oi.Order.Status == "Completed"
                          && oi.Order.CompletedAt >= from
                          && oi.Order.CompletedAt <= to)
                .GroupBy(oi => new { oi.ProductName, oi.Sku })
                .Select(g => new TopProductItem(
                    g.Key.ProductName,
                    g.Key.Sku ?? "",
                    g.Sum(x => x.Quantity),
                    g.Sum(x => x.LineTotal)))
                .OrderByDescending(x => x.TotalQty)
                .Take(top)
                .ToListAsync();
        }

        public async Task<List<InventoryValueItem>> GetInventoryValueAsync()
        {
            return await _db.Inventories
                .Include(i => i.Variant)
                    .ThenInclude(v => v.Product)
                .Select(i => new InventoryValueItem(
                    i.Variant.Product.Name,
                    i.Variant.Sku ?? "",
                    i.Quantity,
                    i.AvgCost,
                    i.Quantity * i.AvgCost))
                .OrderByDescending(x => x.TotalValue)
                .ToListAsync();
        }

        public async Task<byte[]> ExportCsvAsync(string reportType, DateTime from, DateTime to)
        {
            var sb = new StringBuilder();

            if (reportType == "revenue")
            {
                sb.AppendLine("Period,Revenue,Orders");
                var rows = await GetRevenueAsync(from, to, "month");
                foreach (var r in rows)
                    sb.AppendLine($"{r.Period},{r.Revenue},{r.Orders}");
            }
            else if (reportType == "top-products")
            {
                sb.AppendLine("ProductName,Sku,TotalQty,TotalRevenue");
                var rows = await GetTopProductsAsync(from, to, 50);
                foreach (var r in rows)
                    sb.AppendLine($"\"{r.ProductName}\",{r.Sku},{r.TotalQty},{r.TotalRevenue}");
            }
            else if (reportType == "inventory-value")
            {
                sb.AppendLine("ProductName,Sku,Quantity,AvgCost,TotalValue");
                var rows = await GetInventoryValueAsync();
                foreach (var r in rows)
                    sb.AppendLine($"\"{r.ProductName}\",{r.Sku},{r.Quantity},{r.AvgCost},{r.TotalValue}");
            }

            return Encoding.UTF8.GetBytes(sb.ToString());
        }
    }
}