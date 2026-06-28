
    namespace FashionERP.Application.Common
    {
        using System;

        /// <summary>Exception nghiệp vụ - sẽ được bắt tại GlobalExceptionMiddleware</summary>
        public class AppException : Exception
        {
            public int StatusCode { get; }

            public AppException(string message, int statusCode = 400)
                : base(message)
            {
                StatusCode = statusCode;
            }
        }

        /// <summary>Không tìm thấy resource → HTTP 404</summary>
        public class NotFoundException : AppException
        {
            public NotFoundException(string resource, object id)
                : base($"Không tìm thấy {resource} với Id = {id}", 404) { }
        }

        /// <summary>Trùng lặp dữ liệu → HTTP 409</summary>
        public class DuplicateException : AppException
        {
            public DuplicateException(string message)
                : base(message, 409) { }
        }

        /// <summary>Không có quyền → HTTP 403</summary>
        public class ForbiddenException : AppException
        {
            public ForbiddenException(string message = "Bạn không có quyền thực hiện thao tác này")
                : base(message, 403) { }
        }

        /// <summary>Logic nghiệp vụ vi phạm → HTTP 422</summary>
        public class BusinessException : AppException
        {
            public BusinessException(string message)
                : base(message, 422) { }
        }
    }
