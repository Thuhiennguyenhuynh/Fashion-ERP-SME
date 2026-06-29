using System;
using System.Collections.Generic;

namespace FashionERP.Application.DTOs.Finance
{
    public class CashTransactionResponseDto
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = string.Empty;       // INCOME | EXPENSE
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Note { get; set; }
        public string? RefType { get; set; }
        public Guid? RefId { get; set; }
        public decimal BalanceAfter { get; set; }
        public DateTime TransactionDate { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }
    }

    /// <summary>Dùng khi Accountant/Admin ghi thu/chi thủ công (vd: thu tiền mặt bàn giao, chi mặt bằng)</summary>
    public class CreateCashTransactionRequestDto
    {
        public string Type { get; set; } = string.Empty;       // "INCOME" | "EXPENSE"
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string? Note { get; set; }
        public DateTime? TransactionDate { get; set; }
    }

    public class CashBalanceResponseDto
    {
        public decimal CurrentBalance { get; set; }
        public DateTime AsOf { get; set; }
    }

    public class ProfitLossReportDto
    {
        public decimal Revenue { get; set; }
        public decimal Cogs { get; set; }
        public decimal GrossProfit { get; set; }
        public decimal GrossMarginPercent { get; set; }
        public List<ProfitByProductItem> ProfitByProduct { get; set; } = new();
    }

    public class ProfitByProductItem
    {
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public int QtySold { get; set; }
        public decimal Revenue { get; set; }
        public decimal Cogs { get; set; }
        public decimal GrossProfit { get; set; }
    }

    public class CashFlowReportItem
    {
        public string Period { get; set; } = string.Empty;     // vd "2026-W26" hoặc "2026-06"
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
        public decimal Net { get; set; }
    }

    public class ExpenseByCategoryItem
    {
        public string Category { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public int Count { get; set; }
    }
}
