using System;
using System.Collections.Generic;

namespace FashionERP.Application.DTOs.Product
{
    public class CreateProductRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public string Gender { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string? Tags { get; set; }
        public string Status { get; set; } = "Draft";
    }

    public class UpdateProductRequestDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public string Gender { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string? Tags { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class ProductResponseDto
    {
        public Guid Id { get; set; }
        public string ProductCode { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string? BrandName { get; set; }
        public string Gender { get; set; } = string.Empty;
        public decimal BasePrice { get; set; }
        public string? MainImageUrl { get; set; }
        public string? Tags { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public List<ProductVariantResponseDto> Variants { get; set; } = new();
    }

    public class CreateVariantRequestDto
    {
        public Guid ProductId { get; set; }
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string? ColorHex { get; set; }
        public decimal? Price { get; set; }
        public string? Barcode { get; set; }
    }

    public class ProductVariantResponseDto
    {
        public Guid Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Size { get; set; } = string.Empty;
        public string Color { get; set; } = string.Empty;
        public string? ColorHex { get; set; }
        public decimal? Price { get; set; }
        public string? Barcode { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; }
        public int StockQuantity { get; set; }
    }
}
