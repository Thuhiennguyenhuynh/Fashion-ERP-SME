using System;
using System.Threading.Tasks;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Finance;
using FashionERP.Domain.Enums;

namespace FashionERP.Application.Interfaces
{
    public interface ICashTransactionService
    {
        Task<PagedResult<CashTransactionResponseDto>> GetAllAsync(
            string? type, string? category, DateTime? from, DateTime? to, int page, int pageSize);

        Task<CashBalanceResponseDto> GetCurrentBalanceAsync();

        /// <summary>Ghi thu/chi thủ công (Accountant/Admin gọi trực tiếp qua API)</summary>
        Task<CashTransactionResponseDto> CreateManualAsync(CreateCashTransactionRequestDto request, Guid createdBy);

        /// <summary>
        /// Dùng chung — mọi service khác (OrderService, PurchaseOrderService, PayrollService)
        /// gọi hàm này để ghi sổ quỹ, đảm bảo BalanceAfter luôn được tính nối tiếp đúng.
        /// </summary>
        Task<CashTransactionResponseDto> RecordAsync(
            CashTransactionType type, string category, decimal amount,
            string? refType, Guid? refId, string? note, Guid? createdBy);

        // ===== Reports =====
        Task<ProfitLossReportDto> GetProfitLossAsync(DateTime from, DateTime to);
        Task<System.Collections.Generic.List<CashFlowReportItem>> GetCashFlowAsync(DateTime from, DateTime to, string groupBy);
        Task<System.Collections.Generic.List<ExpenseByCategoryItem>> GetExpensesByCategoryAsync(DateTime from, DateTime to);
    }
}