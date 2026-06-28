using System;
using System.ComponentModel.DataAnnotations;
using FashionERP.Domain.Common;
using FashionERP.Domain.Enums;

namespace FashionERP.Domain.Entities
{
    /// <summary>
    /// Đơn nghỉ phép của nhân viên
    /// </summary>
    public class Leave : BaseEntity
    {
        [Required(ErrorMessage = "Nhân viên không được để trống")]
        public Guid EmployeeId { get; set; }
        public virtual Employee Employee { get; set; } = null!;

        [Required(ErrorMessage = "Ngày bắt đầu nghỉ không được để trống")]
        public DateTime FromDate { get; set; }

        [Required(ErrorMessage = "Ngày kết thúc nghỉ không được để trống")]
        public DateTime ToDate { get; set; }

        [Required(ErrorMessage = "Số ngày nghỉ không được để trống")]
        [Range(1, int.MaxValue, ErrorMessage = "Số ngày nghỉ phải lớn hơn 0")]
        public int Days { get; set; }

        [Required(ErrorMessage = "Lý do nghỉ không được để trống")]
        [StringLength(300, MinimumLength = 3,
            ErrorMessage = "Lý do nghỉ phải có độ dài từ 3 đến 300 ký tự")]
        public string Reason { get; set; } = string.Empty;

        [EnumDataType(typeof(LeaveStatus), ErrorMessage = "Trạng thái phải là Pending, Approved hoặc Rejected")]
        public LeaveStatus Status { get; set; } = LeaveStatus.Pending;

        /// <summary>Người duyệt (nullable - chưa duyệt)</summary>
        public Guid? ApprovedBy { get; set; }
        public virtual Employee? Approver { get; set; }
    }
}

