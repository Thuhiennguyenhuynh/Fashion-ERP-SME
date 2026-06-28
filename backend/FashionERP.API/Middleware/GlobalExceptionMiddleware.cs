namespace FashionERP.API.Middleware
{
    using System;
    using System.Text.Json;
    using System.Threading.Tasks;
    using FluentValidation;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using FashionERP.Application.Common;

    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next,
            ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ValidationException ex)
            {
                // FluentValidation errors → 400
                var errors = new System.Collections.Generic.List<string>();
                foreach (var error in ex.Errors)
                    errors.Add(error.ErrorMessage);

                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(
                    ApiResponse.Fail("Dữ liệu không hợp lệ", errors)));
            }
            catch (AppException ex)
            {
                context.Response.StatusCode = ex.StatusCode;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(
                    ApiResponse.Fail(ex.Message)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Lỗi hệ thống không xác định");
                context.Response.StatusCode = 500;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(JsonSerializer.Serialize(
                    ApiResponse.Fail("Lỗi hệ thống, vui lòng thử lại sau")));
            }
        }
    }
}


