using Microsoft.Extensions.DependencyInjection;
using FashionERP.Application.Interfaces;
using FashionERP.Infrastructure.Auth;
using FashionERP.Infrastructure.Services;


namespace FashionERP.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(
            this IServiceCollection services)
        {
            // ── Auth ──────────────────────────────────────────
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

            // ── Core Business Services ────────────────────────
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<IInventoryService, InventoryService>();
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IOrderService, OrderService>();
            services.AddScoped<IPromotionService, PromotionService>();

            // ── HR Services ───────────────────────────────────
           services.AddScoped<IAttendanceService, AttendanceService>();
           services.AddScoped<ILeaveService, LeaveService>();
            services.AddScoped<IPayrollService, PayrollService>();

            // ── External Services ─────────────────────────────
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<IReportService, ReportService>();


            return services;
        }
    }
}

