using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Thương hiệu sản phẩm (có logo lưu trên Cloudinary)
    /// </summary>
    public class Brand : BaseEntity
    {
        [Required(ErrorMessage = "Tên thương hiệu không được để trống")]
        [StringLength(100, MinimumLength = 1,
            ErrorMessage = "Tên thương hiệu phải có độ dài từ 1 đến 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        // ☁ Cloudinary
        [StringLength(500)]
        [Url(ErrorMessage = "URL logo không hợp lệ")]
        public string? LogoUrl { get; set; }

        [StringLength(200)]
        public string? LogoPublicId { get; set; }

        [StringLength(100, ErrorMessage = "Xuất xứ không được vượt quá 100 ký tự")]
        public string? Country { get; set; }

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

