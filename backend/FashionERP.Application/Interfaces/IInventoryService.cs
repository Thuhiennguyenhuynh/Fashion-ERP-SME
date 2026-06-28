namespace FashionERP.Application.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FashionERP.Application.Common;
    using FashionERP.Application.DTOs.Inventory;

    public interface IInventoryService
    {
        Task<PagedResult<InventoryResponseDto>> GetAllAsync(
       bool? lowStockOnly, Guid? productId, string? keyword,
       int page, int pageSize);
        Task<InventoryResponseDto> GetByVariantIdAsync(Guid variantId);
        Task ImportStockAsync(ImportStockRequestDto request, Guid createdBy);
        Task AdjustStockAsync(AdjustStockRequestDto request, Guid createdBy);

    }
}
