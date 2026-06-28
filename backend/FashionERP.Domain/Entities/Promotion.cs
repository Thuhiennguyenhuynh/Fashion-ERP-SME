using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;
using FashionERP.Domain.Enums;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Khuyến mãi / Voucher
    /// </summary>
    public class Promotion : BaseEntity
    {
        [Required(ErrorMessage = "Mã khuyến mãi không được để trống")]
        [StringLength(50)]
        [RegularExpression(ValidationConstants.PromotionCodePattern,
            ErrorMessage = "Mã khuyến mãi chỉ được chứa chữ hoa và số, từ 3 đến 50 ký tự, không khoảng trắng")]
        public string Code { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tên chương trình không được để trống")]
        [StringLength(200, MinimumLength = 3,
            ErrorMessage = "Tên chương trình phải có độ dài từ 3 đến 200 ký tự")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Loại giảm giá không được để trống")]
        [EnumDataType(typeof(PromotionType), ErrorMessage = "Loại giảm giá phải là Percent hoặc FixedAmount")]
        public PromotionType Type { get; set; }

        [Required(ErrorMessage = "Giá trị giảm không được để trống")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Giá trị giảm phải lớn hơn 0")]
        public decimal DiscountValue { get; set; }

        /// <summary>Giảm tối đa - áp dụng khi Type = Percent</summary>
        [Range(0, double.MaxValue, ErrorMessage = "Giảm tối đa phải >= 0")]
        public decimal? MaxDiscount { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Đơn tối thiểu phải >= 0")]
        public decimal MinOrderValue { get; set; } = 0;

        /// <summary>Giới hạn lượt dùng. NULL = không giới hạn</summary>
        [Range(1, int.MaxValue, ErrorMessage = "Giới hạn lượt dùng phải lớn hơn 0")]
        public int? UsageLimit { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Số lần đã dùng phải >= 0")]
        public int UsedCount { get; set; } = 0;

        [Required(ErrorMessage = "Ngày bắt đầu không được để trống")]
        public DateTime StartDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc không được để trống")]
        public DateTime EndDate { get; set; }

        public bool IsActive { get; set; } = true;

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}

