using System;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Tồn kho hiện tại theo từng variant (1 variant - 1 dòng, UNIQUE VariantId)
    /// </summary>
    public class Inventory : BaseEntity
    {
        [Required(ErrorMessage = "Biến thể sản phẩm không được để trống")]
        public Guid VariantId { get; set; }
        public virtual ProductVariant Variant { get; set; } = null!;

        [Range(0, int.MaxValue, ErrorMessage = "Số lượng tồn kho phải >= 0")]
        public int Quantity { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Tồn tối thiểu phải >= 0")]
        public int MinStock { get; set; } = 5;

        [Range(0, int.MaxValue, ErrorMessage = "Tồn tối đa phải >= 0")]
        public int? MaxStock { get; set; }

        [StringLength(100, ErrorMessage = "Vị trí kho không được vượt quá 100 ký tự")]
        public string? Location { get; set; }

        /// <summary>Giá vốn trung bình (moving average)</summary>
        [Range(0, double.MaxValue, ErrorMessage = "Giá vốn trung bình phải >= 0")]
        public decimal AvgCost { get; set; } = 0;

        public DateTime? LastImportDate { get; set; }
    }
}

