using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;
using FashionERP.Domain.Enums;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Nhân viên (có ảnh đại diện lưu trên Cloudinary)
    /// </summary>
    public class Employee : BaseEntity, ISoftDeletable
    {
        [Required(ErrorMessage = "Họ và tên không được để trống")]
        [StringLength(150, MinimumLength = 2,
            ErrorMessage = "Họ và tên phải có độ dài từ 2 đến 150 ký tự")]
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

        [Required(ErrorMessage = "Phòng ban không được để trống")]
        public Guid DepartmentId { get; set; }
        public virtual Department Department { get; set; } = null!;

        [Required(ErrorMessage = "Chức vụ không được để trống")]
        [StringLength(100)]
        public string Position { get; set; } = string.Empty;

        [Range(0, double.MaxValue, ErrorMessage = "Lương cơ bản phải >= 0")]
        public decimal BaseSalary { get; set; } = 0;

        [Range(1, 31, ErrorMessage = "Số công chuẩn/tháng phải từ 1 đến 31")]
        public int WorkingDaysPerMonth { get; set; } = 26;

        [Required(ErrorMessage = "Ngày vào làm không được để trống")]
        public DateTime StartDate { get; set; }

        [EnumDataType(typeof(EmployeeStatus), ErrorMessage = "Trạng thái phải là Active, Probation hoặc Resigned")]
        public EmployeeStatus Status { get; set; } = EmployeeStatus.Probation;

        // ☁ Cloudinary
        [StringLength(500)]
        [Url(ErrorMessage = "URL ảnh đại diện không hợp lệ")]
        public string? AvatarUrl { get; set; }

        [StringLength(200)]
        public string? AvatarPublicId { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        // Navigation
        public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();
        public virtual ICollection<Leave> Leaves { get; set; } = new List<Leave>();
        public virtual ICollection<Payroll> Payrolls { get; set; } = new List<Payroll>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual User? User { get; set; }
    }
}

