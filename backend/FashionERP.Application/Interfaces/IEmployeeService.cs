namespace FashionERP.Application.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FashionERP.Application.Common;
    using FashionERP.Application.DTOs.Employee;

    public interface IEmployeeService
    {
        Task<PagedResult<EmployeeResponseDto>> GetAllAsync(EmployeeQueryParams p);
        Task<EmployeeResponseDto> GetByIdAsync(Guid id);
        Task<EmployeeResponseDto> CreateAsync(CreateEmployeeRequestDto request);
        Task UpdateStatusAsync(Guid id, string status);
        Task<EmployeeResponseDto> UpdateAsync(Guid id, UpdateEmployeeRequestDto request);
        Task DeleteAsync(Guid id);
        Task UpdateAvatarAsync(Guid id, string imageUrl, string publicId);
    }
}

