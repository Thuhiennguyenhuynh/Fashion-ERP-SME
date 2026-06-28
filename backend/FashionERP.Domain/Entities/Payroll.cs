using System;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;
using FashionERP.Domain.Enums;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Bảng lương tháng. Ràng buộc UNIQUE (employeeId, month, year) cấu hình tại DbContext.
    /// </summary>
    public class Payroll : BaseEntity
    {
        [Required(ErrorMessage = "Nhân viên không được để trống")]
        public Guid EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        [Required(ErrorMessage = "Tháng không được để trống")]
        [Range(1, 12, ErrorMessage = "Tháng phải từ 1 đến 12")]
        public int Month { get; set; }

        [Required(ErrorMessage = "Năm không được để trống")]
        [Range(2000, 2100, ErrorMessage = "Năm phải từ 2000 đến 2100")]
        public int Year { get; set; }

        [Required(ErrorMessage = "Số công thực tế không được để trống")]
        [Range(0, 31, ErrorMessage = "Số công thực tế phải từ 0 đến 31")]
        public decimal WorkingDaysActual { get; set; }

        /// <summary>Snapshot lương cơ bản tại thời điểm tính lương</summary>
        [Range(0, double.MaxValue, ErrorMessage = "Lương cơ bản phải >= 0")]
        public decimal BaseSalary { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Phụ cấp phải >= 0")]
        public decimal Allowance { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Tiền làm thêm giờ phải >= 0")]
        public decimal OvertimePay { get; set; } = 0;

        [Range(0, double.MaxValue, ErrorMessage = "Khấu trừ phải >= 0")]
        public decimal Deduction { get; set; } = 0;

        /// <summary>
        /// Tính: (baseSalary / workingDaysPerMonth * workingDaysActual)
        ///       + allowance + overtimePay - deduction
        /// </summary>
        [Range(0, double.MaxValue, ErrorMessage = "Lương thực nhận phải >= 0")]
        public decimal NetSalary { get; set; }

        [EnumDataType(typeof(PayrollStatus), ErrorMessage = "Trạng thái phải là Draft, Confirmed hoặc Paid")]
        public PayrollStatus Status { get; set; } = PayrollStatus.Draft;
    }
}

