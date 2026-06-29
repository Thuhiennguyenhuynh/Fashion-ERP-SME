using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Customer;
using FashionERP.Application.Interfaces;
using FashionERP.Domain.Entities;
using FashionERP.Domain.Enums;
using FashionERP.Infrastructure.Data;

namespace FashionERP.Infrastructure.Services
{
    public class CustomerService : ICustomerService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public CustomerService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public async Task<PagedResult<CustomerResponseDto>> GetAllAsync(CustomerQueryParams p)
        {
            var query = _db.Customers
                .Where(c => !c.IsDeleted)
                .Include(c => c.Measurement)
                .AsQueryable();

            if (!string.IsNullOrEmpty(p.Keyword))
            {
                var kw = p.Keyword.Trim().ToLower();
                query = query.Where(c =>
                    c.FullName.ToLower().Contains(kw) ||
                    c.Phone.Contains(kw) ||
                    (c.Email != null && c.Email.ToLower().Contains(kw)));
            }

            if (!string.IsNullOrEmpty(p.Gender) &&
                Enum.TryParse<Gender>(p.Gender, out var gender))
                query = query.Where(c => c.Gender == gender);

            if (!string.IsNullOrEmpty(p.MemberLevel) &&
                Enum.TryParse<MemberLevel>(p.MemberLevel, out var level))
                query = query.Where(c => c.MemberLevel == level);

            if (p.MinSpent.HasValue)
                query = query.Where(c => c.TotalSpent >= p.MinSpent.Value);
            if (p.MaxSpent.HasValue)
                query = query.Where(c => c.TotalSpent <= p.MaxSpent.Value);

            query = query.OrderByDescending(c => c.CreatedAt);

            var paged = await query.ToPagedResultAsync(p.Page, p.PageSize);
            return new PagedResult<CustomerResponseDto>
            {
                Items = _mapper.Map<List<CustomerResponseDto>>(paged.Items),
                TotalCount = paged.TotalCount,
                Page = paged.Page,
                PageSize = paged.PageSize,
            };
        }

        public async Task<CustomerResponseDto> GetByIdAsync(Guid id)
        {
            var customer = await _db.Customers
                .Where(c => !c.IsDeleted)
                .Include(c => c.Measurement)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException("Khách hàng", id);
            return _mapper.Map<CustomerResponseDto>(customer);
        }

        public async Task<PagedResult<object>> GetOrdersByCustomerAsync(
            Guid customerId, int page, int pageSize)
        {
            _ = await _db.Customers
                .Where(c => !c.IsDeleted)
                .FirstOrDefaultAsync(c => c.Id == customerId)
                ?? throw new NotFoundException("Khách hàng", customerId);

            var query = _db.Orders
                .Where(o => o.CustomerId == customerId)
                .OrderByDescending(o => o.CreatedAt)
                .Select(o => (object)new
                {
                    o.Id,
                    o.OrderCode,
                    Status = o.Status.ToString(),
                    o.FinalAmount,
                    PaymentMethod = o.PaymentMethod.ToString(),
                    o.CreatedAt,
                    ItemCount = o.Items.Count,
                });

            return await query.ToPagedResultAsync(page, pageSize);
        }

        public async Task<CustomerResponseDto> CreateAsync(CreateCustomerRequestDto request)
        {
            if (await _db.Customers.AnyAsync(c => c.Phone == request.Phone.Trim() && !c.IsDeleted))
                throw new DuplicateException($"Số điện thoại '{request.Phone}' đã được đăng ký");

            if (!string.IsNullOrEmpty(request.Email) &&
                await _db.Customers.AnyAsync(c => c.Email == request.Email.Trim().ToLower() && !c.IsDeleted))
                throw new DuplicateException($"Email '{request.Email}' đã được đăng ký");

            Gender? gender = null;
            if (!string.IsNullOrEmpty(request.Gender) && Enum.TryParse<Gender>(request.Gender, out var g))
                gender = g;

            var customer = new Customer
            {
                FullName = request.FullName.Trim(),
                Phone = request.Phone.Trim(),
                Email = request.Email?.Trim().ToLower(),
                Gender = gender,
                DateOfBirth = request.DateOfBirth,
                Address = request.Address?.Trim(),
                Note = request.Note?.Trim(),
                MemberLevel = MemberLevel.Bronze,
                TotalSpent = 0,
                TotalOrders = 0,
            };

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();
            return _mapper.Map<CustomerResponseDto>(customer);
        }

        public async Task<CustomerResponseDto> UpdateAsync(Guid id, UpdateCustomerRequestDto request)
        {
            var customer = await _db.Customers
                .Where(c => !c.IsDeleted)
                .Include(c => c.Measurement)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException("Khách hàng", id);

            if (await _db.Customers.AnyAsync(c => c.Phone == request.Phone.Trim()
                    && c.Id != id && !c.IsDeleted))
                throw new DuplicateException($"Số điện thoại '{request.Phone}' đã được đăng ký");

            if (!string.IsNullOrEmpty(request.Email) &&
                await _db.Customers.AnyAsync(c => c.Email == request.Email.Trim().ToLower()
                    && c.Id != id && !c.IsDeleted))
                throw new DuplicateException($"Email '{request.Email}' đã được đăng ký");

            Gender? gender = null;
            if (!string.IsNullOrEmpty(request.Gender) && Enum.TryParse<Gender>(request.Gender, out var g))
                gender = g;

            customer.FullName = request.FullName.Trim();
            customer.Phone = request.Phone.Trim();
            customer.Email = request.Email?.Trim().ToLower();
            customer.Gender = gender;
            customer.DateOfBirth = request.DateOfBirth;
            customer.Address = request.Address?.Trim();
            customer.Note = request.Note?.Trim();
            customer.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return _mapper.Map<CustomerResponseDto>(customer);
        }

        public async Task DeleteAsync(Guid id)
        {
            var customer = await _db.Customers
                .Where(c => !c.IsDeleted)
                .FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException("Khách hàng", id);

            customer.IsDeleted = true;
            customer.DeletedAt = DateTime.UtcNow;
            customer.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        public async Task SaveMeasurementAsync(Guid customerId, CustomerMeasurementDto dto)
        {
            if (!await _db.Customers.AnyAsync(c => c.Id == customerId && !c.IsDeleted))
                throw new NotFoundException("Khách hàng", customerId);

            var existing = await _db.CustomerMeasurements
                .FirstOrDefaultAsync(m => m.CustomerId == customerId);

            if (existing == null)
            {
                _db.CustomerMeasurements.Add(new CustomerMeasurement
                {
                    CustomerId = customerId,
                    Height = dto.Height,
                    Weight = dto.Weight,
                    Chest = dto.Chest,
                    Waist = dto.Waist,
                    Hip = dto.Hip,
                    UpdatedAtMeasurement = DateTime.UtcNow,
                });
            }
            else
            {
                existing.Height = dto.Height;
                existing.Weight = dto.Weight;
                existing.Chest = dto.Chest;
                existing.Waist = dto.Waist;
                existing.Hip = dto.Hip;
                existing.UpdatedAtMeasurement = DateTime.UtcNow;
                existing.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
        }

        public static MemberLevel CalcMemberLevel(decimal totalSpent) => totalSpent switch
        {
            >= 50_000_000 => MemberLevel.Platinum,
            >= 20_000_000 => MemberLevel.Gold,
            >= 5_000_000 => MemberLevel.Silver,
            _ => MemberLevel.Bronze,
        };
    }
}