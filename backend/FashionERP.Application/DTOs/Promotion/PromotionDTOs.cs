namespace FashionERP.Application.DTOs.Promotion
{
    using System;
    using FashionERP.Application.Common;

    public class CreatePromotionRequestDto
    {
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal MinOrderValue { get; set; } = 0;
        public int? UsageLimit { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
    }

    public class PromotionResponseDto
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public decimal DiscountValue { get; set; }
        public decimal? MaxDiscount { get; set; }
        public decimal MinOrderValue { get; set; }
        public int? UsageLimit { get; set; }
        public int UsedCount { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsActive { get; set; }
    }

    public class ApplyPromotionRequestDto
    {
        public string Code { get; set; } = string.Empty;
        public decimal OrderSubtotal { get; set; }
    }

    public class ApplyPromotionResponseDto
    {
        public bool IsValid { get; set; }
        public string? ErrorMessage { get; set; }
        public decimal DiscountAmount { get; set; }
        public string PromotionName { get; set; } = string.Empty;
    }
    public class PromotionQueryParams : PaginationParams
    {
        public string? Type { get; set; }        // Percent | FixedAmount
        public bool? IsActive { get; set; }
        public DateTime? ValidOn { get; set; }   // lọc khuyến mãi còn hiệu lực tại ngày này
    }
}

