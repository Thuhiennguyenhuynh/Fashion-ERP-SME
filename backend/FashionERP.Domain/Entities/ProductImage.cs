using System;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Ảnh phụ / gallery của sản phẩm (Cloudinary). CASCADE DELETE theo Product.
    /// </summary>
    public class ProductImage : BaseEntity
    {
        [Required(ErrorMessage = "Sản phẩm không được để trống")]
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; } = null!;

        // ☁ Cloudinary
        [Required(ErrorMessage = "URL ảnh không được để trống")]
        [StringLength(500)]
        [Url(ErrorMessage = "URL ảnh không hợp lệ")]
        public string ImageUrl { get; set; } = string.Empty;

        [Required(ErrorMessage = "PublicId ảnh không được để trống")]
        [StringLength(200)]
        public string PublicId { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Alt text không được vượt quá 200 ký tự")]
        public string? AltText { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị phải >= 0")]
        public int SortOrder { get; set; } = 0;

        /// <summary>=1 nếu là ảnh chính (sync với Products.MainImageUrl)</summary>
        public bool IsMain { get; set; } = false;
    }
}

