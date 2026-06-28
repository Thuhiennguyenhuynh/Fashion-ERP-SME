using System;

namespace FashionERP.Application.DTOs.Customer
{
    public class CreateCustomerRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
    }

    public class UpdateCustomerRequestDto
    {
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? Note { get; set; }
    }

    public class CustomerResponseDto
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string? Email { get; set; }
        public string? Gender { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Address { get; set; }
        public string? AvatarUrl { get; set; }
        public string MemberLevel { get; set; } = string.Empty;
        public decimal TotalSpent { get; set; }
        public int TotalOrders { get; set; }
        public string? Note { get; set; }
        public CustomerMeasurementDto? Measurement { get; set; }
    }

    public class CustomerMeasurementDto
    {
        public decimal? Height { get; set; }
        public decimal? Weight { get; set; }
        public decimal? Chest { get; set; }
        public decimal? Waist { get; set; }
        public decimal? Hip { get; set; }
    }
}
