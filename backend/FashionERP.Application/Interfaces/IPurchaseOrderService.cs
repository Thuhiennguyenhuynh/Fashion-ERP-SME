using System;
using System.Threading.Tasks;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Procurement;

namespace FashionERP.Application.Interfaces
{
    public interface IPurchaseOrderService
    {
        Task<PagedResult<PurchaseOrderResponseDto>> GetAllAsync(string? status, Guid? supplierId, DateTime? from, DateTime? to, int page, int pageSize);
        Task<PurchaseOrderResponseDto> GetByIdAsync(Guid id);

        // 1. Tạo PO ở trạng thái Draft
        Task<PurchaseOrderResponseDto> CreateAsync(CreatePurchaseOrderRequestDto request, Guid createdBy);

        // 2. Chuyển từ Draft -> Ordered (Xác nhận đặt hàng với NCC)
        Task ConfirmOrderAsync(Guid id);

        // 3. Nhập kho hàng hóa (Cộng Inventory, tính AvgCost, ghi công nợ)
        Task<PurchaseOrderResponseDto> ReceiveItemsAsync(Guid id, ReceivePurchaseOrderRequestDto request, Guid receivedBy);

        // 4. Kế toán thanh toán tiền cho NCC
        Task PaySupplierAsync(Guid id, PayPurchaseOrderRequestDto request, Guid paidBy);

        // 5. Hủy phiếu (Chỉ cho phép khi chưa nhận hàng)
        Task CancelAsync(Guid id);
    }
}