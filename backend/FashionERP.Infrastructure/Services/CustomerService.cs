using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        private IQueryable<Customer> BaseQuery() =>
            _db.Customers.Include(c => c.Measurement);

        // ─── GET ALL ──────────────────────────────────────────
        public async Task<List<CustomerResponseDto>> GetAllAsync(string? keyword)
        {
            var query = BaseQuery();
            if (!string.IsNullOrEmpty(keyword))
            {
                var kw = keyword.Trim().ToLower();
                query = query.Where(c =>
                    c.FullName.ToLower().Contains(kw) ||
                    c.Phone.Contains(kw) ||
                    (c.Email != null && c.Email.ToLower().Contains(kw)));
            }
            var list = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
            return _mapper.Map<List<CustomerResponseDto>>(list);
        }

        // ─── GET BY ID ────────────────────────────────────────
        public async Task<CustomerResponseDto> GetByIdAsync(Guid id)
        {
            var customer = await BaseQuery().FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException("Khách hàng", id);
            return _mapper.Map<CustomerResponseDto>(customer);
        }

        // ─── CREATE ───────────────────────────────────────────
        public async Task<CustomerResponseDto> CreateAsync(CreateCustomerRequestDto request)
        {
            if (await _db.Customers.AnyAsync(c => c.Phone == request.Phone.Trim()))
                throw new DuplicateException($"Số điện thoại '{request.Phone}' đã được đăng ký bởi khách hàng khác");

            if (!string.IsNullOrEmpty(request.Email) &&
                await _db.Customers.AnyAsync(c => c.Email == request.Email.Trim().ToLower()))
                throw new DuplicateException($"Email '{request.Email}' đã được đăng ký bởi khách hàng khác");

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
                TotalOrders = 0
            };

            _db.Customers.Add(customer);
            await _db.SaveChangesAsync();
            return _mapper.Map<CustomerResponseDto>(customer);
        }

        // ─── UPDATE ───────────────────────────────────────────
        public async Task<CustomerResponseDto> UpdateAsync(Guid id, UpdateCustomerRequestDto request)
        {
            var customer = await BaseQuery().FirstOrDefaultAsync(c => c.Id == id)
                ?? throw new NotFoundException("Khách hàng", id);

            if (await _db.Customers.AnyAsync(c => c.Phone == request.Phone.Trim() && c.Id != id))
                throw new DuplicateException($"Số điện thoại '{request.Phone}' đã được đăng ký bởi khách hàng khác");

            if (!string.IsNullOrEmpty(request.Email) &&
                await _db.Customers.AnyAsync(c => c.Email == request.Email.Trim().ToLower() && c.Id != id))
                throw new DuplicateException($"Email '{request.Email}' đã được đăng ký bởi khách hàng khác");

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

        // ─── DELETE ───────────────────────────────────────────
        public async Task DeleteAsync(Guid id)
        {
            var customer = await _db.Customers.FindAsync(id)
                ?? throw new NotFoundException("Khách hàng", id);

            // Trả về lỗi quy trình nếu khách hàng đã có đơn hàng (Không cần dùng IsActive)
            if (await _db.Orders.AnyAsync(o => o.CustomerId == id))
                throw new BusinessException("Không thể xóa khách hàng đã có lịch sử đơn hàng.");

            _db.Customers.Remove(customer);
            await _db.SaveChangesAsync();
        }

        // ─── SAVE MEASUREMENT ─────────────────────────────────
        public async Task SaveMeasurementAsync(Guid customerId, CustomerMeasurementDto dto)
        {
            if (!await _db.Customers.AnyAsync(c => c.Id == customerId))
                throw new NotFoundException("Khách hàng", customerId);

            var existing = await _db.CustomerMeasurements
                .FirstOrDefaultAsync(m => m.CustomerId == customerId);

            if (existing == null)
            {
                var m = new CustomerMeasurement
                {
                    CustomerId = customerId,
                    Height = dto.Height,
                    Weight = dto.Weight,
                    Chest = dto.Chest,
                    Waist = dto.Waist,
                    Hip = dto.Hip,
                    UpdatedAtMeasurement = DateTime.UtcNow
                };
                _db.CustomerMeasurements.Add(m);
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

        // ─── HELPER: Tự động nâng hạng member ────────────────
        public static MemberLevel CalcMemberLevel(decimal totalSpent) => totalSpent switch
        {
            >= 50_000_000 => MemberLevel.Platinum,
            >= 20_000_000 => MemberLevel.Gold,
            >= 5_000_000 => MemberLevel.Silver,
            _ => MemberLevel.Bronze
        };
    }
}


