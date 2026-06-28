namespace FashionERP.Application.DTOs.Order
{
    using System;
    using System.Collections.Generic;

    public class CreateOrderRequestDto
    {
        public Guid? CustomerId { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? PromotionCode { get; set; }
        public string? Note { get; set; }
        public List<CreateOrderItemDto> Items { get; set; } = new();
    }

    public class CreateOrderItemDto
    {
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
    }

    public class OrderResponseDto
    {
        public Guid Id { get; set; }
        public string OrderCode { get; set; } = string.Empty;
        public string? CustomerName { get; set; }
        public string StaffName { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal FinalAmount { get; set; }
        public string PaymentMethod { get; set; } = string.Empty;
        public string? PromotionCode { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Note { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public List<OrderItemResponseDto> Items { get; set; } = new();
    }

    public class OrderItemResponseDto
    {
        public Guid VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class CreateReturnRequestDto
    {
        public Guid OrderId { get; set; }
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string ReturnType { get; set; } = string.Empty;
        public decimal? RefundAmount { get; set; }
    }
}

