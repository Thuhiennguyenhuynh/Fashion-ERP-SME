namespace FashionERP.Application.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FashionERP.Application.Common;
    using FashionERP.Application.DTOs.Product;

    public interface IProductService
    {
        Task<PagedResult<ProductResponseDto>> GetAllAsync(ProductQueryParams p);
        Task<ProductResponseDto> GetByIdAsync(Guid id);
        Task<ProductResponseDto> CreateAsync(CreateProductRequestDto request);
        Task<ProductResponseDto> UpdateAsync(Guid id, UpdateProductRequestDto request);
        Task DeleteAsync(Guid id);
        Task UpdateMainImageAsync(Guid id, string imageUrl, string publicId);

        Task<ProductVariantResponseDto> AddVariantAsync(CreateVariantRequestDto request);
        Task<ProductVariantResponseDto> UpdateVariantAsync(Guid variantId, CreateVariantRequestDto request);
        Task DeleteVariantAsync(Guid variantId);
    }
}
