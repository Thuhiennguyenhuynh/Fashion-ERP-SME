using System;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;
using FashionERP.Domain.Enums;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Lịch sử giao dịch kho (audit trail đầy đủ trước/sau)
    /// </summary>
    public class InventoryTransaction : BaseEntity
    {
        [Required(ErrorMessage = "Biến thể sản phẩm không được để trống")]
        public Guid VariantId { get; set; }
        public virtual ProductVariant Variant { get; set; } = null!;

        [Required(ErrorMessage = "Loại giao dịch không được để trống")]
        [EnumDataType(typeof(InventoryTransactionType),
            ErrorMessage = "Loại giao dịch phải là IMPORT, EXPORT, ADJUST hoặc RETURN")]
        public InventoryTransactionType Type { get; set; }

        /// <summary>+ nhập / - xuất, không được = 0</summary>
        [Required(ErrorMessage = "Số lượng không được để trống")]
        [NotZero(ErrorMessage = "Số lượng giao dịch không được bằng 0")]
        public int Quantity { get; set; }

        /// <summary>Giá nhập (chỉ có khi IMPORT)</summary>
        [Range(0, double.MaxValue, ErrorMessage = "Giá nhập phải >= 0")]
        public decimal? UnitCost { get; set; }

        /// <summary>Nguồn gốc: Order, ManualImport, Stocktake, Return</summary>
        [StringLength(30)]
        public string? RefType { get; set; }

        /// <summary>ID của đơn hàng / phiếu nhập liên quan</summary>
        public Guid? RefId { get; set; }

        [Required(ErrorMessage = "Tồn kho trước giao dịch không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Tồn kho trước giao dịch phải >= 0")]
        public int QuantityBefore { get; set; }

        [Required(ErrorMessage = "Tồn kho sau giao dịch không được để trống")]
        [Range(0, int.MaxValue, ErrorMessage = "Tồn kho sau giao dịch phải >= 0")]
        public int QuantityAfter { get; set; }

        [StringLength(300, ErrorMessage = "Ghi chú không được vượt quá 300 ký tự")]
        public string? Note { get; set; }

        [Required(ErrorMessage = "Người tạo giao dịch không được để trống")]
        public virtual User Creator { get; set; } = null!;
    }
}

