using System;
using System.Threading.Tasks;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Procurement;

namespace FashionERP.Application.Interfaces
{
    public interface ISupplierService
    {
        Task<PagedResult<SupplierResponseDto>> GetAllAsync(string? keyword, bool? isActive, int page, int pageSize);
        Task<SupplierResponseDto> GetByIdAsync(Guid id);
        Task<SupplierResponseDto> CreateAsync(CreateSupplierRequestDto request);
        Task<SupplierResponseDto> UpdateAsync(Guid id, CreateSupplierRequestDto request);
        Task ToggleActiveAsync(Guid id);
    }
}