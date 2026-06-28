using System;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;
using FashionERP.Domain.Enums;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Biến thể sản phẩm theo Size x Color. SKU và Barcode tự sinh.
    /// UNIQUE (ProductId, Size, Color) cấu hình tại DbContext.
    /// </summary>
    public class ProductVariant : BaseEntity
    {
        [Required(ErrorMessage = "Sản phẩm không được để trống")]
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        [Required(ErrorMessage = "Kích cỡ không được để trống")]
        [EnumDataType(typeof(ProductSize),
            ErrorMessage = "Kích cỡ phải là XS, S, M, L, XL, XXL, XXXL hoặc FREE")]
        public ProductSize Size { get; set; }

        [Required(ErrorMessage = "Màu sắc không được để trống")]
        [StringLength(50, MinimumLength = 1,
            ErrorMessage = "Màu sắc phải có độ dài từ 1 đến 50 ký tự")]
        public string Color { get; set; } = string.Empty;

        [StringLength(7)]
        [RegularExpression(ValidationConstants.ColorHexPattern,
            ErrorMessage = "Mã màu HEX phải có dạng #RRGGBB, ví dụ: #FF0000")]
        public string? ColorHex { get; set; }

        /// <summary>SKU tự sinh: PROD001-M-RED</summary>
        [Required(ErrorMessage = "SKU không được để trống")]
        [StringLength(100)]
        [RegularExpression(ValidationConstants.SkuPattern,
            ErrorMessage = "SKU phải có định dạng MÃSP-SIZE-MÀU (chữ hoa/số), ví dụ: PROD001-M-RED")]
        public string Sku { get; set; } = string.Empty;

        /// <summary>Barcode EAN-13 (tự sinh hoặc nhập tay)</summary>
        [StringLength(20)]
        [RegularExpression(ValidationConstants.BarcodePattern,
            ErrorMessage = "Barcode phải là mã EAN-13 gồm đúng 13 số")]
        public string? Barcode { get; set; }

        /// <summary>Giá riêng của variant (NULL -> fallback Products.BasePrice)</summary>
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá phải lớn hơn 0")]
        public decimal? Price { get; set; }

        // ☁ Cloudinary - ảnh riêng variant (NULL -> fallback Products.MainImageUrl)
        [StringLength(500)]
        [Url(ErrorMessage = "URL ảnh không hợp lệ")]
        public string? ImageUrl { get; set; }

        [StringLength(200)]
        public string? ImagePublicId { get; set; }

        public bool IsActive { get; set; } = true;

        // Navigation
        public virtual Inventory? Inventory { get; set; }
        public virtual ICollection<InventoryTransaction> InventoryTransactions { get; set; } = new List<InventoryTransaction>();
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public virtual ICollection<Return> Returns { get; set; } = new List<Return>();
    }
}

