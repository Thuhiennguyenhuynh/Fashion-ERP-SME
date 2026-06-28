using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;
using FashionERP.Domain.Enums;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Khách hàng (có ảnh đại diện lưu trên Cloudinary, member level tự cập nhật theo TotalSpent)
    /// </summary>
    public class Customer : BaseEntity
    {
        [Required(ErrorMessage = "Tên khách hàng không được để trống")]
        [StringLength(150, MinimumLength = 2,
            ErrorMessage = "Tên khách hàng phải có độ dài từ 2 đến 150 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [StringLength(15)]
        [RegularExpression(ValidationConstants.PhonePattern,
            ErrorMessage = "Số điện thoại phải là số Việt Nam hợp lệ gồm 10 số, bắt đầu bằng số 0")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(255)]
        [RegularExpression(ValidationConstants.EmailPattern,
            ErrorMessage = "Email không đúng định dạng (ví dụ: ten@example.com)")]
        public string? Email { get; set; }

        [EnumDataType(typeof(Gender), ErrorMessage = "Giới tính phải là Male, Female hoặc Other")]
        public Gender? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(300, ErrorMessage = "Địa chỉ không được vượt quá 300 ký tự")]
        public string? Address { get; set; }

        // ☁ Cloudinary
        [StringLength(500)]
        [Url(ErrorMessage = "URL ảnh đại diện không hợp lệ")]
        public string? AvatarUrl { get; set; }

        [StringLength(200)]
        public string? AvatarPublicId { get; set; }

        [EnumDataType(typeof(MemberLevel),
            ErrorMessage = "Cấp độ thành viên phải là Bronze, Silver, Gold hoặc Platinum")]
        public MemberLevel MemberLevel { get; set; } = MemberLevel.Bronze;

        [Range(0, double.MaxValue, ErrorMessage = "Tổng chi tiêu phải >= 0")]
        public decimal TotalSpent { get; set; } = 0;

        [Range(0, int.MaxValue, ErrorMessage = "Tổng số đơn hàng phải >= 0")]
        public int TotalOrders { get; set; } = 0;

        [StringLength(300, ErrorMessage = "Ghi chú không được vượt quá 300 ký tự")]
        public string? Note { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation
        public virtual CustomerMeasurement? Measurement { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
