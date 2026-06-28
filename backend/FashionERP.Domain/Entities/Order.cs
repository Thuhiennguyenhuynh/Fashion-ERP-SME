using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;
using FashionERP.Domain.Enums;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Đơn hàng. OrderCode tự sinh dạng ORD-YYYYMMDD-XXX.
    /// </summary>
    public class Order : BaseEntity
    {
        [Required(ErrorMessage = "Mã đơn hàng không được để trống")]
        [StringLength(30)]
        [RegularExpression(ValidationConstants.OrderCodePattern,
            ErrorMessage = "Mã đơn hàng phải có định dạng ORD-YYYYMMDD-XXX, ví dụ: ORD-20250601-001")]
        public string OrderCode { get; set; } = string.Empty;

        /// <summary>NULL = khách lẻ</summary>
        public Guid? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }

        [Required(ErrorMessage = "Nhân viên tạo đơn không được để trống")]
        public Guid StaffId { get; set; }
        public virtual Employee Staff { get; set; } = null!;

        [Required(ErrorMessage = "Tổng tiền trước giảm giá không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Tổng tiền trước giảm giá phải >= 0")]
        public decimal Subtotal { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Số tiền giảm phải >= 0")]
        public decimal DiscountAmount { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Thuế VAT phải >= 0")]
        public decimal TaxAmount { get; set; } = 0;

        /// <summary>= Subtotal - DiscountAmount + TaxAmount</summary>
        [Required(ErrorMessage = "Tổng tiền thanh toán không được để trống")]
        [Range(0, double.MaxValue, ErrorMessage = "Tổng tiền thanh toán phải >= 0")]
        public decimal FinalAmount { get; set; }

        [Required(ErrorMessage = "Phương thức thanh toán không được để trống")]
        [EnumDataType(typeof(PaymentMethod), ErrorMessage = "Phương thức thanh toán phải là Cash, Transfer hoặc Card")]
        public PaymentMethod PaymentMethod { get; set; }

        public Guid? PromotionId { get; set; }
        public virtual Promotion? Promotion { get; set; }

        /// <summary>Lưu code lúc đặt (snapshot)</summary>
        [StringLength(50)]
        public string? PromotionCode { get; set; }

        [EnumDataType(typeof(OrderStatus),
            ErrorMessage = "Trạng thái phải là Pending, Completed, Cancelled hoặc Returned")]
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        [StringLength(300, ErrorMessage = "Ghi chú không được vượt quá 300 ký tự")]
        public string? Note { get; set; }

        /// <summary>Thời điểm hoàn tất thanh toán</summary>
        public DateTime? CompletedAt { get; set; }

        // Navigation
        public virtual ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public virtual ICollection<Return> Returns { get; set; } = new List<Return>();
    }
}

