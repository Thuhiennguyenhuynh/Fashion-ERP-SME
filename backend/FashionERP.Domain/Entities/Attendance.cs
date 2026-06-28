using System;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;
using FashionERP.Domain.Enums;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Chấm công theo ngày. Ràng buộc UNIQUE (employeeId, workDate) cấu hình tại DbContext.
    /// </summary>
    public class Attendance : BaseEntity
    {
        [Required(ErrorMessage = "Nhân viên không được để trống")]
        public Guid EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        [Required(ErrorMessage = "Ngày làm việc không được để trống")]
        public DateTime WorkDate { get; set; }

        public DateTime? CheckIn { get; set; }

        public DateTime? CheckOut { get; set; }

        /// <summary>Tính khi check-out: (CheckOut - CheckIn)</summary>
        [Range(0, 24, ErrorMessage = "Tổng giờ làm phải từ 0 đến 24")]
        public decimal? TotalHours { get; set; }

        [Range(0, 24, ErrorMessage = "Giờ làm thêm phải từ 0 đến 24")]
        public decimal OvertimeHours { get; set; } = 0;

        [Required(ErrorMessage = "Loại chấm công không được để trống")]
        [EnumDataType(typeof(AttendanceType),
            ErrorMessage = "Loại chấm công phải là Normal, Late, EarlyLeave, Absent hoặc Holiday")]
        public AttendanceType Type { get; set; } = AttendanceType.Normal;

        [StringLength(300, ErrorMessage = "Ghi chú không được vượt quá 300 ký tự")]
        public string? Note { get; set; }
    }
}

