using System;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;
using FashionERP.Domain.Enums;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Đổi trả hàng
    /// </summary>
    public class Return : BaseEntity
    {
        [Required(ErrorMessage = "Đơn hàng không được để trống")]
        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; } = null!;

        [Required(ErrorMessage = "Biến thể sản phẩm không được để trống")]
        public Guid VariantId { get; set; }
        public virtual ProductVariant Variant { get; set; } = null!;

        [Required(ErrorMessage = "Số lượng không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }

        [Required(ErrorMessage = "Lý do trả hàng không được để trống")]
        [StringLength(300, MinimumLength = 3,
            ErrorMessage = "Lý do trả hàng phải có độ dài từ 3 đến 300 ký tự")]
        public string Reason { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hình thức đổi trả không được để trống")]
        [EnumDataType(typeof(ReturnType), ErrorMessage = "Hình thức đổi trả phải là Refund hoặc Exchange")]
        public ReturnType ReturnType { get; set; }

        /// <summary>Chỉ áp dụng khi ReturnType = Refund</summary>
        [Range(0, double.MaxValue, ErrorMessage = "Số tiền hoàn phải >= 0")]
        public decimal? RefundAmount { get; set; }

        [EnumDataType(typeof(ReturnStatus), ErrorMessage = "Trạng thái phải là Pending hoặc Completed")]
        public ReturnStatus Status { get; set; } = ReturnStatus.Pending;

        [Required(ErrorMessage = "Người tạo phiếu đổi trả không được để trống")]
        public Guid CreatedBy { get; set; }
        public virtual User Creator { get; set; } = null!;
    }
}

