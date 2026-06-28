namespace FashionERP.Application.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FashionERP.Application.Common;
    using FashionERP.Application.DTOs.Order;

    public interface IOrderService
    {
        Task<PagedResult<OrderResponseDto>> GetAllAsync(
    string? status, DateTime? from, DateTime? to,
    Guid? staffId, int page, int pageSize);
        Task<OrderResponseDto> GetByIdAsync(Guid id);
        Task<OrderResponseDto> CreateAsync(CreateOrderRequestDto request, Guid staffId);
        Task CancelAsync(Guid id);
        Task CompleteAsync(Guid id);
        Task<OrderResponseDto> CreateReturnAsync(CreateReturnRequestDto request, Guid createdBy);
    }
}
