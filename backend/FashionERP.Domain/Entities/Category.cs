using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Danh mục sản phẩm (hỗ trợ nested categories qua ParentId)
    /// </summary>
    public class Category : BaseEntity
    {
        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [StringLength(100, MinimumLength = 2,
            ErrorMessage = "Tên danh mục phải có độ dài từ 2 đến 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Slug không được để trống")]
        [StringLength(100)]
        [RegularExpression(ValidationConstants.SlugPattern,
            ErrorMessage = "Slug chỉ được chứa chữ thường, số và dấu gạch ngang, ví dụ: ao-thun-nam")]
        public string Slug { get; set; } = string.Empty;

        [StringLength(300, ErrorMessage = "Mô tả không được vượt quá 300 ký tự")]
        public string? Description { get; set; }

        /// <summary>Danh mục cha (nested categories)</summary>
        public Guid? ParentId { get; set; }
        public virtual Category? Parent { get; set; }
        public virtual ICollection<Category> Children { get; set; } = new List<Category>();

        [Range(0, int.MaxValue, ErrorMessage = "Thứ tự hiển thị phải >= 0")]
        public int SortOrder { get; set; } = 0;

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}

