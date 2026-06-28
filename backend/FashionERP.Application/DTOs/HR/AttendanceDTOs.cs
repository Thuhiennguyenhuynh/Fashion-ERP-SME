using System;

namespace FashionERP.Application.DTOs.HR
{
    public class CheckInRequestDto
    {
        public Guid EmployeeId { get; set; }
        public string? Note { get; set; }
    }

    public class CheckOutRequestDto
    {
        public Guid EmployeeId { get; set; }
        public DateTime WorkDate { get; set; }
    }

    public class CreateAttendanceManualDto
    {
        public Guid EmployeeId { get; set; }
        public DateTime WorkDate { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public string Type { get; set; } = "Normal";
        public string? Note { get; set; }
    }

    public class AttendanceResponseDto
    {
        public Guid Id { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime WorkDate { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public decimal? TotalHours { get; set; }
        public decimal OvertimeHours { get; set; }
        public string Type { get; set; } = string.Empty;
        public string? Note { get; set; }
    }

    public class CreateLeaveRequestDto
    {
        public Guid EmployeeId { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public string Reason { get; set; } = string.Empty;
    }

    public class ApproveLeaveRequestDto
    {
        public string Status { get; set; } = string.Empty; // Approved / Rejected
        public string? Note { get; set; }
    }

    public class LeaveResponseDto
    {
        public Guid Id { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int Days { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ApproverName { get; set; }
    }

    public class GeneratePayrollRequestDto
    {
        public Guid EmployeeId { get; set; }
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal Allowance { get; set; } = 0;
        public decimal Deduction { get; set; } = 0;
    }

    public class PayrollResponseDto
    {
        public Guid Id { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public int Month { get; set; }
        public int Year { get; set; }
        public decimal WorkingDaysActual { get; set; }
        public decimal BaseSalary { get; set; }
        public decimal Allowance { get; set; }
        public decimal OvertimePay { get; set; }
        public decimal Deduction { get; set; }
        public decimal NetSalary { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}

