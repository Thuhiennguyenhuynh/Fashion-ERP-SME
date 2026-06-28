using AutoMapper;
using FashionERP.Application.DTOs.Auth;
using FashionERP.Application.DTOs.Customer;
using FashionERP.Application.DTOs.Employee;
using FashionERP.Application.DTOs.HR;
using FashionERP.Application.DTOs.Inventory;
using FashionERP.Application.DTOs.Order;
using FashionERP.Application.DTOs.Product;
using FashionERP.Application.DTOs.Promotion;
using FashionERP.Domain.Entities;

namespace FashionERP.Application.Mappings
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            // ===== Auth / User =====
            CreateMap<User, UserInfoDto>()
                .ForMember(d => d.Role, o => o.MapFrom(s => s.Role.ToString()))
                .ForMember(d => d.FullName, o => o.MapFrom(s => s.Employee != null ? s.Employee.FullName : null))
                .ForMember(d => d.AvatarUrl, o => o.MapFrom(s => s.Employee != null ? s.Employee.AvatarUrl : null));

            // ===== Employee =====
            CreateMap<Employee, EmployeeResponseDto>()
                .ForMember(d => d.DepartmentName, o => o.MapFrom(s => s.Department.Name))
                .ForMember(d => d.Gender, o => o.MapFrom(s => s.Gender.HasValue ? s.Gender.ToString() : null))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

            // ===== Attendance =====
            CreateMap<Attendance, AttendanceResponseDto>()
                .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee.FullName))
                .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

            // ===== Leave =====
            CreateMap<Leave, LeaveResponseDto>()
                .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee.FullName))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
                .ForMember(d => d.ApproverName, o => o.MapFrom(s => s.Approver != null ? s.Approver.FullName : null));

            // ===== Payroll =====
            CreateMap<Payroll, PayrollResponseDto>()
                .ForMember(d => d.EmployeeName, o => o.MapFrom(s => s.Employee.FullName))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

            // ===== Product =====
            CreateMap<Product, ProductResponseDto>()
                .ForMember(d => d.CategoryName, o => o.MapFrom(s => s.Category.Name))
                .ForMember(d => d.BrandName, o => o.MapFrom(s => s.Brand != null ? s.Brand.Name : null))
                .ForMember(d => d.Gender, o => o.MapFrom(s => s.Gender.ToString()))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

            // ===== ProductVariant =====
            CreateMap<ProductVariant, ProductVariantResponseDto>()
                .ForMember(d => d.Size, o => o.MapFrom(s => s.Size.ToString()))
                .ForMember(d => d.StockQuantity,
                    o => o.MapFrom(s => s.Inventory != null ? s.Inventory.Quantity : 0));

            // ===== Inventory =====
            CreateMap<Inventory, InventoryResponseDto>()
                .ForMember(d => d.ProductName, o => o.MapFrom(s => s.Variant.Product.Name))
                .ForMember(d => d.Sku, o => o.MapFrom(s => s.Variant.Sku))
                .ForMember(d => d.Size, o => o.MapFrom(s => s.Variant.Size.ToString()))
                .ForMember(d => d.Color, o => o.MapFrom(s => s.Variant.Color));

            // ===== Customer =====
            CreateMap<Customer, CustomerResponseDto>()
                .ForMember(d => d.Gender, o => o.MapFrom(s => s.Gender.HasValue ? s.Gender.ToString() : null))
                .ForMember(d => d.MemberLevel, o => o.MapFrom(s => s.MemberLevel.ToString()));

            CreateMap<CustomerMeasurement, CustomerMeasurementDto>();

            // ===== Promotion =====
            CreateMap<Promotion, PromotionResponseDto>()
                .ForMember(d => d.Type, o => o.MapFrom(s => s.Type.ToString()));

            // ===== Order =====
            CreateMap<Order, OrderResponseDto>()
                .ForMember(d => d.CustomerName,
                    o => o.MapFrom(s => s.Customer != null ? s.Customer.FullName : null))
                .ForMember(d => d.StaffName, o => o.MapFrom(s => s.Staff.FullName))
                .ForMember(d => d.PaymentMethod, o => o.MapFrom(s => s.PaymentMethod.ToString()))
                .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()));

            CreateMap<OrderItem, OrderItemResponseDto>();
        }
    }
}


