namespace FashionERP.Application.Interfaces
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using FashionERP.Application.Common;
    using FashionERP.Application.DTOs.Promotion;

    public interface IPromotionService
    {
        Task<PagedResult<PromotionResponseDto>> GetAllAsync(PromotionQueryParams p);

        Task<PromotionResponseDto> CreateAsync(CreatePromotionRequestDto request);
        Task DeactivateAsync(Guid id);
        Task<ApplyPromotionResponseDto> ApplyCodeAsync(ApplyPromotionRequestDto request);
    }
}
