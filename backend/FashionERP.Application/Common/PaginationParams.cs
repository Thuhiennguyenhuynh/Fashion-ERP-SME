using System;

namespace FashionERP.Application.Common
{
    /// <summary>Base class cho mọi QueryParams (ProductQueryParams, CustomerQueryParams...).
    /// Kế thừa class này thay vì khai lại Page/PageSize/SortBy/Keyword ở từng module.</summary>
    public class PaginationParams
    {
        private const int MaxPageSize = 100;
        private int _pageSize = 20;
        private int _page = 1;

        public int Page
        {
            get => _page;
            set => _page = value < 1 ? 1 : value;
        }

        public int PageSize
        {
            get => _pageSize;
            set => _pageSize = value <= 0 ? 20 : Math.Min(value, MaxPageSize);
        }

        /// <summary>VD: "createdAt_desc", "name_asc"</summary>
        public string? SortBy { get; set; }

        /// <summary>Từ khóa tìm kiếm thông minh (đa từ khóa, đa field)</summary>
        public string? Keyword { get; set; }
    }
}