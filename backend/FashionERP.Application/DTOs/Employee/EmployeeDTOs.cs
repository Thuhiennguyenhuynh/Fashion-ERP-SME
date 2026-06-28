using System;
using FashionERP.Domain.Enums;

namespace FashionERP.Application.DTOs.Employee
{
    public class CreateEmployeeRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public Guid DepartmentId { get; set; }
        public string Position { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
        public int WorkingDaysPerMonth { get; set; } = 26;
        public DateTime StartDate { get; set; }
    }

    public class UpdateEmployeeRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public Guid DepartmentId { get; set; }
        public string Position { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
        public int WorkingDaysPerMonth { get; set; }
        public string Status { get; set; } = string.Empty;
    }

    public class EmployeeResponseDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string DepartmentName { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public decimal BaseSalary { get; set; }
        public int WorkingDaysPerMonth { get; set; }
        public DateTime StartDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? AvatarUrl { get; set; }
    }
}

