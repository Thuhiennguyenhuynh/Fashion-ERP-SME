using System;
using System.Collections.Generic;
using FashionERP.Application.Common;

namespace FashionERP.Application.DTOs.Procurement
{
    // ===================== SUPPLIER DTOs =====================
    public class SupplierResponseDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? TaxCode { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public decimal TotalDebt { get; set; }
        public string? Note { get; set; }
        public bool IsActive { get; set; }
    }

    public class CreateSupplierRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string? ContactPerson { get; set; }
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? TaxCode { get; set; }
        public string? BankAccount { get; set; }
        public string? BankName { get; set; }
        public string? Note { get; set; }
    }

    // ===================== PURCHASE ORDER DTOs =====================
    public class PurchaseOrderResponseDto
    {
        public Guid Id { get; set; }
        public string PoCode { get; set; } = string.Empty;
        public Guid SupplierId { get; set; }
        public string SupplierName { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public decimal PaidAmount { get; set; }
        public decimal DebtAmount { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public DateTime? ReceivedDate { get; set; }
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public Guid? CreatedBy { get; set; }

        public List<PurchaseOrderItemResponseDto> Items { get; set; } = new();
    }

    public class PurchaseOrderItemResponseDto
    {
        public Guid Id { get; set; }
        public Guid VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int OrderedQty { get; set; }
        public int ReceivedQty { get; set; }
        public decimal UnitCost { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class CreatePurchaseOrderRequestDto
    {
        public Guid SupplierId { get; set; }
        public DateTime? ExpectedDate { get; set; }
        public string? Note { get; set; }
        public List<CreatePoItemDto> Items { get; set; } = new();
    }

    public class CreatePoItemDto
    {
        public Guid VariantId { get; set; }
        public int OrderedQty { get; set; }
        public decimal UnitCost { get; set; }
    }

    // Dùng để thủ kho xác nhận số lượng thực tế nhận được khi hàng về
    public class ReceivePurchaseOrderRequestDto
    {
        public List<ReceivePoItemDto> Items { get; set; } = new();
    }

    public class ReceivePoItemDto
    {
        public Guid PurchaseOrderItemId { get; set; }
        public int ReceivedQtyThisTime { get; set; } // Số lượng nhận trong lần này
    }

    // Dùng để kế toán ghi nhận thanh toán cho NCC
    public class PayPurchaseOrderRequestDto
    {
        public decimal Amount { get; set; }
        public string? Note { get; set; }
    }
}