using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Customer;

namespace FashionERP.Application.Interfaces
{
    public interface ICustomerService
    {
        Task<PagedResult<CustomerResponseDto>> GetAllAsync(CustomerQueryParams p);

        Task<CustomerResponseDto> GetByIdAsync(Guid id);
        Task<CustomerResponseDto> CreateAsync(CreateCustomerRequestDto request);
        Task<CustomerResponseDto> UpdateAsync(Guid id, UpdateCustomerRequestDto request);
        Task DeleteAsync(Guid id);
        Task SaveMeasurementAsync(Guid customerId, CustomerMeasurementDto dto);

        Task<PagedResult<object>> GetOrdersByCustomerAsync(Guid customerId, int page, int pageSize);
    }
}