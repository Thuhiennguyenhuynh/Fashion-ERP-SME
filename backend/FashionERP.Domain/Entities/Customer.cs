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
    public class Customer : BaseEntity, ISoftDeletable   // ✅ THÊM INTERFACE
    {
        [Required(ErrorMessage = "Tên khách hàng không được để trống")]
        [StringLength(150, MinimumLength = 2,
            ErrorMessage = "Tên khách hàng phải có độ dài từ 2 đến 150 ký tự")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [StringLength(15)]
        [RegularExpression(ValidationConstants.PhonePattern,
            ErrorMessage = "Số điện thoại phải là số Việt Nam hợp lệ")]
        public string Phone { get; set; } = string.Empty;

        [StringLength(255)]
        [RegularExpression(ValidationConstants.EmailPattern,
            ErrorMessage = "Email không đúng định dạng")]
        public string? Email { get; set; }

        [EnumDataType(typeof(Gender))]
        public Gender? Gender { get; set; }

        public DateTime? DateOfBirth { get; set; }

        [StringLength(300)]
        public string? Address { get; set; }

        // ☁ Avatar
        [StringLength(500)]
        [Url]
        public string? AvatarUrl { get; set; }

        [StringLength(200)]
        public string? AvatarPublicId { get; set; }

        [EnumDataType(typeof(MemberLevel))]
        public MemberLevel MemberLevel { get; set; } = MemberLevel.Bronze;

        public decimal TotalSpent { get; set; } = 0;
        public int TotalOrders { get; set; } = 0;

        [StringLength(300)]
        public string? Note { get; set; }

        // ✅ Soft Delete
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation
        public virtual CustomerMeasurement? Measurement { get; set; }
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}