using System.Collections.Generic;

namespace FashionERP.Application.Common
{
    /// <summary>
    /// Wrapper chuẩn cho mọi response API trả về FE.
    /// Thành công: { success=true, data=..., message=... }
    /// Thất bại:   { success=false, message=..., errors=[...] }
    /// </summary>
    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public T? Data { get; set; }
        public List<string>? Errors { get; set; }

        public static ApiResponse<T> Ok(T data, string message = "Thành công")
            => new() { Success = true, Data = data, Message = message };

        public static ApiResponse<T> Fail(string message, List<string>? errors = null)
            => new() { Success = false, Message = message, Errors = errors };
    }

    public class ApiResponse : ApiResponse<object>
    {
        public static ApiResponse Ok(string message = "Thành công")
            => new() { Success = true, Message = message };

        public new static ApiResponse Fail(string message, List<string>? errors = null)
            => new() { Success = false, Message = message, Errors = errors };
    }
}

