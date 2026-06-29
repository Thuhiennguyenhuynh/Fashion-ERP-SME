using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Finance;
using FashionERP.Application.Interfaces;
using FashionERP.Domain.Entities;
using FashionERP.Domain.Enums;
using FashionERP.Infrastructure.Data;

namespace FashionERP.Infrastructure.Services
{
    public class CashTransactionService : ICashTransactionService
    {
        private readonly AppDbContext _db;

        public CashTransactionService(AppDbContext db)
        {
            _db = db;
        }

        // ───────────────────────────────────────────────────────
        // GET ALL / BALANCE
        // ───────────────────────────────────────────────────────
        public async Task<PagedResult<CashTransactionResponseDto>> GetAllAsync(
            string? type, string? category, DateTime? from, DateTime? to, int page, int pageSize)
        {
            var query = _db.CashTransactions.AsQueryable();

            if (!string.IsNullOrEmpty(type) && Enum.TryParse<CashTransactionType>(type, true, out var parsedType))
                query = query.Where(t => t.Type == parsedType);

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(t => t.Category == category);

            if (from.HasValue)
                query = query.Where(t => t.TransactionDate >= from.Value);

            if (to.HasValue)
                query = query.Where(t => t.TransactionDate <= to.Value);

            var total = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.TransactionDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(t => Map(t))
                .ToListAsync();

            return new PagedResult<CashTransactionResponseDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<CashBalanceResponseDto> GetCurrentBalanceAsync()
        {
            var last = await _db.CashTransactions
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.CreatedAt)
                .FirstOrDefaultAsync();

            return new CashBalanceResponseDto
            {
                CurrentBalance = last?.BalanceAfter ?? 0,
                AsOf = DateTime.UtcNow
            };
        }

        // ───────────────────────────────────────────────────────
        // CREATE (manual + dùng chung cho service khác)
        // ───────────────────────────────────────────────────────
        public async Task<CashTransactionResponseDto> CreateManualAsync(
            CreateCashTransactionRequestDto request, Guid createdBy)
        {
            if (!Enum.TryParse<CashTransactionType>(request.Type, true, out var type))
                throw new AppException("Loại giao dịch không hợp lệ, phải là INCOME hoặc EXPENSE");

            var result = await RecordAsync(
                type, request.Category.Trim(), request.Amount,
                refType: "Manual", refId: null, note: request.Note?.Trim(), createdBy);

            return result;
        }

        /// <summary>
        /// Hàm dùng CHUNG cho mọi nơi cần ghi sổ quỹ (OrderService khi Completed,
        /// PurchaseOrderService khi PaySupplier, PayrollService khi MarkAsPaid...).
        /// Đảm bảo BalanceAfter luôn nối tiếp đúng theo giao dịch gần nhất.
        /// </summary>
        public async Task<CashTransactionResponseDto> RecordAsync(
            CashTransactionType type, string category, decimal amount,
            string? refType, Guid? refId, string? note, Guid? createdBy)
        {
            if (amount <= 0)
                throw new BusinessException("Số tiền giao dịch phải lớn hơn 0");

            var lastBalance = await _db.CashTransactions
                .OrderByDescending(t => t.TransactionDate)
                .ThenByDescending(t => t.CreatedAt)
                .Select(t => t.BalanceAfter)
                .FirstOrDefaultAsync();

            var newBalance = type == CashTransactionType.INCOME
                ? lastBalance + amount
                : lastBalance - amount;

            var entity = new CashTransaction
            {
                Type = type,
                Category = category,
                Amount = amount,
                Note = note,
                RefType = refType,
                RefId = refId,
                BalanceAfter = newBalance,
                TransactionDate = DateTime.UtcNow,
                CreatedBy = createdBy
            };

            _db.CashTransactions.Add(entity);
            await _db.SaveChangesAsync();

            return Map(entity);
        }

        // ───────────────────────────────────────────────────────
        // REPORTS
        // ───────────────────────────────────────────────────────

        /// <summary>
        /// Revenue lấy từ Order.FinalAmount (Completed). COGS lấy từ
        /// OrderItem.Quantity * OrderItem.UnitCostSnapshot (snapshot AvgCost lúc bán,
        /// KHÔNG dùng Inventory.AvgCost hiện tại để tránh sai lệch do giá nhập sau thay đổi).
        /// Yêu cầu: OrderItem phải có field UnitCostSnapshot (xem ghi chú tích hợp).
        /// </summary>
        public async Task<ProfitLossReportDto> GetProfitLossAsync(DateTime from, DateTime to)
        {
            var orderItems = await _db.OrderItems
                .Include(oi => oi.Order)
                .Include(oi => oi.Variant)
                .Where(oi => oi.Order.Status == OrderStatus.Completed
                          && oi.Order.CompletedAt >= from
                          && oi.Order.CompletedAt <= to)
                .Select(oi => new
                {
                    oi.ProductName,
                    Sku = oi.Variant.Sku,
                    oi.Quantity,
                    oi.LineTotal,
                    Cogs = oi.Quantity * oi.UnitCostSnapshot
                })
                .ToListAsync();

            var revenue = orderItems.Sum(x => x.LineTotal);
            var cogs = orderItems.Sum(x => x.Cogs);
            var grossProfit = revenue - cogs;

            var byProduct = orderItems
                .GroupBy(x => new { x.ProductName, x.Sku })
                .Select(g => new ProfitByProductItem
                {
                    ProductName = g.Key.ProductName,
                    Sku = g.Key.Sku ?? "",
                    QtySold = g.Sum(x => x.Quantity),
                    Revenue = g.Sum(x => x.LineTotal),
                    Cogs = g.Sum(x => x.Cogs),
                    GrossProfit = g.Sum(x => x.LineTotal) - g.Sum(x => x.Cogs)
                })
                .OrderByDescending(x => x.GrossProfit)
                .ToList();

            return new ProfitLossReportDto
            {
                Revenue = revenue,
                Cogs = cogs,
                GrossProfit = grossProfit,
                GrossMarginPercent = revenue == 0 ? 0 : Math.Round(grossProfit / revenue * 100, 2),
                ProfitByProduct = byProduct
            };
        }

        public async Task<List<CashFlowReportItem>> GetCashFlowAsync(DateTime from, DateTime to, string groupBy)
        {
            var transactions = await _db.CashTransactions
                .Where(t => t.TransactionDate >= from && t.TransactionDate <= to)
                .Select(t => new { t.TransactionDate, t.Type, t.Amount })
                .ToListAsync();

            string KeyOf(DateTime d) => groupBy.ToLower() switch
            {
                "day" => d.ToString("yyyy-MM-dd"),
                "week" => ISOWeek.GetYear(d) + "-W" + ISOWeek.GetWeekOfYear(d).ToString("D2"),
                _ => d.ToString("yyyy-MM")
            };

            var grouped = transactions
                .GroupBy(t => KeyOf(t.TransactionDate))
                .Select(g => new CashFlowReportItem
                {
                    Period = g.Key,
                    Income = g.Where(x => x.Type == CashTransactionType.INCOME).Sum(x => x.Amount),
                    Expense = g.Where(x => x.Type == CashTransactionType.EXPENSE).Sum(x => x.Amount),
                    Net = g.Where(x => x.Type == CashTransactionType.INCOME).Sum(x => x.Amount)
                         - g.Where(x => x.Type == CashTransactionType.EXPENSE).Sum(x => x.Amount)
                })
                .OrderBy(x => x.Period)
                .ToList();

            return grouped;
        }

        public async Task<List<ExpenseByCategoryItem>> GetExpensesByCategoryAsync(DateTime from, DateTime to)
        {
            return await _db.CashTransactions
                .Where(t => t.Type == CashTransactionType.EXPENSE
                         && t.TransactionDate >= from && t.TransactionDate <= to)
                .GroupBy(t => t.Category)
                .Select(g => new ExpenseByCategoryItem
                {
                    Category = g.Key,
                    TotalAmount = g.Sum(x => x.Amount),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.TotalAmount)
                .ToListAsync();
        }

        private static CashTransactionResponseDto Map(CashTransaction t) => new()
        {
            Id = t.Id,
            Type = t.Type.ToString(),
            Category = t.Category,
            Amount = t.Amount,
            Note = t.Note,
            RefType = t.RefType,
            RefId = t.RefId,
            BalanceAfter = t.BalanceAfter,
            TransactionDate = t.TransactionDate,
            CreatedAt = t.CreatedAt,
            CreatedBy = t.CreatedBy
        };
    }
}