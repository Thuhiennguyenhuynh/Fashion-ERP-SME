using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace FashionERP.Application.Common
{
    public static class SortHelper
    {
        /// <summary>sortBy dạng "field_asc" / "field_desc". field tra trong sortMap do từng Service khai báo.
        /// Nếu sortBy null/không hợp lệ -> dùng defaultField + desc.</summary>
        public static IQueryable<T> ApplySort<T>(
            this IQueryable<T> query,
            string? sortBy,
            Dictionary<string, Expression<Func<T, object>>> sortMap,
            string defaultField = "createdat")
        {
            if (sortMap == null || sortMap.Count == 0) return query;

            if (string.IsNullOrWhiteSpace(sortBy))
                sortBy = $"{defaultField}_desc";

            var parts = sortBy.Split('_');
            var field = parts[0].Trim().ToLower();
            var desc = parts.Length > 1 && parts[1].Equals("desc", StringComparison.OrdinalIgnoreCase);

            if (!sortMap.TryGetValue(field, out var expr))
            {
                // fallback an toàn: dùng defaultField nếu có, không thì lấy field đầu tiên trong map
                expr = sortMap.TryGetValue(defaultField, out var defExpr) ? defExpr : sortMap.Values.First();
            }

            return desc ? query.OrderByDescending(expr) : query.OrderBy(expr);
        }
    }
}