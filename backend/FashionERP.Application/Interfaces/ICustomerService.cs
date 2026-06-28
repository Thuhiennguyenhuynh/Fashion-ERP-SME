namespace FashionERP.Application.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FashionERP.Application.DTOs.Customer;

    public interface ICustomerService
    {
        Task<List<CustomerResponseDto>> GetAllAsync(string? keyword);
        Task<CustomerResponseDto> GetByIdAsync(Guid id);
        Task<CustomerResponseDto> CreateAsync(CreateCustomerRequestDto request);
        Task<CustomerResponseDto> UpdateAsync(Guid id, UpdateCustomerRequestDto request);
        Task DeleteAsync(Guid id);
        Task SaveMeasurementAsync(Guid customerId, CustomerMeasurementDto dto);
    }
}
