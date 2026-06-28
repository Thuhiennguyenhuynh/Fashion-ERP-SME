using System;
using FashionERP.Application.Common;

namespace FashionERP.Application.DTOs.Product
{
    /// <summary>Param cho GET /products. Kế thừa Page/PageSize/SortBy/Keyword từ PaginationParams.</summary>
    public class ProductQueryParams : PaginationParams
    {
        public string? Status { get; set; }
        public Guid? CategoryId { get; set; }
        public Guid? BrandId { get; set; }
        public string? Gender { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Size { get; set; }
        public string? Color { get; set; }
        public bool? InStock { get; set; }
    }
}