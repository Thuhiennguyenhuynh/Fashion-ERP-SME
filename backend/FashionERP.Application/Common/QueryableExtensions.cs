using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FashionERP.Application.Common
{
    public static class QueryableExtensions
    {
        /// <summary>Áp Skip/Take + đếm tổng, trả về PagedResult<T> (class gốc, KHÔNG đổi).
        /// Dùng cho mọi Service thay vì viết tay Skip/Take/Count từng nơi.</summary>
        public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
            this IQueryable<T> query, int page, int pageSize, CancellationToken ct = default)
        {
            page = page < 1 ? 1 : page;
            pageSize = pageSize <= 0 ? 20 : pageSize;

            var totalCount = await query.CountAsync(ct);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        /// <summary>Map PagedResult<TSrc> sang PagedResult<TDto>, giữ nguyên thông tin trang.</summary>
        public static PagedResult<TDto> MapTo<TSrc, TDto>(
            this PagedResult<TSrc> source, Func<List<TSrc>, List<TDto>> mapFn)
            => new()
            {
                Items = mapFn(source.Items),
                TotalCount = source.TotalCount,
                Page = source.Page,
                PageSize = source.PageSize
            };

        /// <summary>Chỉ áp Where khi condition = true. Thay cho if/else lặp lại ở mỗi Service.</summary>
        public static IQueryable<T> WhereIf<T>(
            this IQueryable<T> query, bool condition, Expression<Func<T, bool>> predicate)
            => condition ? query.Where(predicate) : query;
    }
}