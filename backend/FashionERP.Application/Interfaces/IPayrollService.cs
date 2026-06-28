namespace FashionERP.Application.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FashionERP.Application.DTOs.HR;

    public interface IPayrollService
    {
        Task<PayrollResponseDto> GenerateAsync(GeneratePayrollRequestDto request);
        Task<List<PayrollResponseDto>> GetByMonthYearAsync(int month, int year);
        Task<PayrollResponseDto?> GetByEmployeeMonthAsync(Guid employeeId, int year, int month);
        Task ConfirmAsync(Guid id);
        Task MarkAsPaidAsync(Guid id);
    }
}

