using System;
using System.Linq;
using System.Linq.Expressions;

namespace FashionERP.Application.Common
{
    public static class SearchHelper
    {
        /// <summary>
        /// Tìm kiếm thông minh: tách keyword thành nhiều token, mỗi token phải khớp
        /// ít nhất 1 trong các field truyền vào (OR theo field), và TẤT CẢ token đều
        /// phải match (AND theo token). VD: "áo xanh" -> phải có "áo" trong field nào đó
        /// VÀ "xanh" trong field nào đó (không nhất thiết cùng field).
        /// Dùng chung cho mọi entity, không phân biệt hoa-thường, không cần Elasticsearch.
        /// </summary>
        public static IQueryable<T> SmartSearch<T>(
            this IQueryable<T> query, string? keyword, params Expression<Func<T, string?>>[] fields)
        {
            if (string.IsNullOrWhiteSpace(keyword) || fields == null || fields.Length == 0)
                return query;

            var tokens = keyword.Trim().ToLower()
                .Split(' ', StringSplitOptions.RemoveEmptyEntries)
                .Distinct();

            foreach (var token in tokens)
            {
                var t = token; // tránh closure bug khi capture biến loop
                Expression<Func<T, bool>>? tokenPredicate = null;

                foreach (var field in fields)
                {
                    var param = field.Parameters[0];
                    var fieldAccess = field.Body;

                    var notNull = Expression.NotEqual(fieldAccess, Expression.Constant(null, typeof(string)));
                    var toLower = Expression.Call(fieldAccess, nameof(string.ToLower), Type.EmptyTypes);
                    var contains = Expression.Call(
                        toLower, nameof(string.Contains), Type.EmptyTypes, Expression.Constant(t));
                    var safeContains = Expression.AndAlso(notNull, contains);
                    var lambda = Expression.Lambda<Func<T, bool>>(safeContains, param);

                    tokenPredicate = tokenPredicate == null ? lambda : Or(tokenPredicate, lambda);
                }

                if (tokenPredicate != null)
                    query = query.Where(tokenPredicate);
            }

            return query;
        }

        private static Expression<Func<T, bool>> Or<T>(Expression<Func<T, bool>> a, Expression<Func<T, bool>> b)
        {
            var param = a.Parameters[0];
            var body = Expression.OrElse(a.Body, Expression.Invoke(b, param));
            return Expression.Lambda<Func<T, bool>>(body, param);
        }
    }
}