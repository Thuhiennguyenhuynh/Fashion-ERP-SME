namespace FashionERP.Application.DTOs.Inventory
{
    using System;

    public class ImportStockRequestDto
    {
        public Guid VariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitCost { get; set; }
        public string? Note { get; set; }
    }

    public class AdjustStockRequestDto
    {
        public Guid VariantId { get; set; }
        public int NewQuantity { get; set; }
        public string? Note { get; set; }
    }

    public class InventoryResponseDto
    {
        public Guid Id { get; set; }
        public Guid VariantId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Sku { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public int MinStock { get; set; }
        public int? MaxStock { get; set; }
        public string? Location { get; set; }
        public decimal AvgCost { get; set; }
        public DateTime? LastImportDate { get; set; }
        public bool IsLowStock => Quantity <= MinStock;
    }
}

