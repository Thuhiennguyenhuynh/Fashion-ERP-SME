using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;
using FashionERP.Domain.Enums;
namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Sản phẩm chính. ProductCode tự sinh dạng PROD-YYYY-XXXX.
    /// </summary>
    public class Product : BaseEntity, ISoftDeletable
    {
        [Required(ErrorMessage = "Tên sản phẩm không được để trống")]
        [StringLength(200, MinimumLength = 2,
            ErrorMessage = "Tên sản phẩm phải có độ dài từ 2 đến 200 ký tự")]
        public string Name { get; set; } = string.Empty;
        /// <summary>Mô tả chi tiết (có thể chứa HTML hoặc plain text)</summary>
        public string? Description { get; set; }
        [Required(ErrorMessage = "Danh mục không được để trống")]
        public Guid CategoryId { get; set; }
        public virtual Category Category { get; set; } = null!;
        public Guid? BrandId { get; set; }
        public virtual Brand? Brand { get; set; }
        [Required(ErrorMessage = "Đối tượng sử dụng không được để trống")]
        [EnumDataType(typeof(ProductGender),
            ErrorMessage = "Đối tượng phải là Male, Female, Unisex hoặc Kids")]
        public ProductGender Gender { get; set; }
        [Required(ErrorMessage = "Giá bán cơ bản không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá bán cơ bản phải lớn hơn 0")]
        public decimal BasePrice { get; set; }
        // ☁ Cloudinary - ảnh chính
        [StringLength(500)]
        [Url(ErrorMessage = "URL ảnh chính không hợp lệ")]
        public string? MainImageUrl { get; set; }
        [StringLength(200)]
        public string? MainImagePublicId { get; set; }
        /// <summary>Tag tìm kiếm, cách nhau bởi dấu phẩy: "sale,new,best-seller"</summary>
        [StringLength(500)]
        public string? Tags { get; set; }
        [EnumDataType(typeof(ProductStatus), ErrorMessage = "Trạng thái phải là Active, Draft hoặc Archived")]
        public ProductStatus Status { get; set; } = ProductStatus.Draft;
        /// <summary>Mã sản phẩm tự sinh: PROD-2025-0001</summary>
        [Required(ErrorMessage = "Mã sản phẩm không được để trống")]
        [StringLength(50)]
        [RegularExpression(ValidationConstants.ProductCodePattern,
            ErrorMessage = "Mã sản phẩm phải có định dạng PROD-YYYY-XXXX, ví dụ: PROD-2025-0001")]
        public string ProductCode { get; set; } = string.Empty;

        // ===== Soft delete (ISoftDeletable) =====
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation
        public virtual ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();
        public virtual ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
    }
}