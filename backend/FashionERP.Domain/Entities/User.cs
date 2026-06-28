using System;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;
using FashionERP.Domain.Enums;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Tài khoản đăng nhập hệ thống (ánh xạ tới Employees nếu là nhân viên)
    /// </summary>
    public class User : BaseEntity
    {
        [Required(ErrorMessage = "Email không được để trống")]
        [StringLength(255, ErrorMessage = "Email không được vượt quá 255 ký tự")]
        [RegularExpression(ValidationConstants.EmailPattern,
            ErrorMessage = "Email không đúng định dạng (ví dụ: ten@example.com)")]
        public string Email { get; set; } = string.Empty;

        /// <summary>BCrypt hash, KHÔNG lưu plaintext</summary>
        [Required(ErrorMessage = "Mật khẩu (hash) không được để trống")]
        [StringLength(512)]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vai trò không được để trống")]
        [EnumDataType(typeof(UserRole), ErrorMessage = "Vai trò phải là một trong: Admin, Manager, Sales, Warehouse, Accountant")]
        public UserRole Role { get; set; }

        /// <summary>Liên kết nhân viên (nullable - tài khoản hệ thống có thể không gắn nhân viên)</summary>
        public Guid? EmployeeId { get; set; }
        public virtual Employee? Employee { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime? LastLoginAt { get; set; }

        [StringLength(512)]
        public string? RefreshToken { get; set; }

        public DateTime? RefreshTokenExpiry { get; set; }
    }
}

