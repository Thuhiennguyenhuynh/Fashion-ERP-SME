using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Procurement;
using FashionERP.Application.Interfaces;
using FashionERP.Domain.Entities;
using FashionERP.Infrastructure.Data;

namespace FashionERP.Infrastructure.Services
{
    public class SupplierService : ISupplierService
    {
        private readonly AppDbContext _db;

        public SupplierService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<PagedResult<SupplierResponseDto>> GetAllAsync(string? keyword, bool? isActive, int page, int pageSize)
        {
            var query = _db.Suppliers.AsQueryable();

            if (isActive.HasValue)
                query = query.Where(s => s.IsActive == isActive.Value);

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.Trim().ToLower();
                query = query.Where(s =>
                    s.Name.ToLower().Contains(lowerKeyword) ||
                    s.Phone.Contains(lowerKeyword) ||
                    (s.Email != null && s.Email.ToLower().Contains(lowerKeyword)));
            }

            var total = await query.CountAsync();
            var items = await query
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(s => new SupplierResponseDto
                {
                    Id = s.Id,
                    Name = s.Name,
                    ContactPerson = s.ContactPerson,
                    Phone = s.Phone,
                    Email = s.Email,
                    Address = s.Address,
                    TaxCode = s.TaxCode,
                    BankAccount = s.BankAccount,
                    BankName = s.BankName,
                    TotalDebt = s.TotalDebt,
                    Note = s.Note,
                    IsActive = s.IsActive
                })
                .ToListAsync();

            return new PagedResult<SupplierResponseDto>
            {
                Items = items,
                TotalCount = total,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<SupplierResponseDto> GetByIdAsync(Guid id)
        {
            var s = await _db.Suppliers.FindAsync(id) ?? throw new NotFoundException("Nhà cung cấp", id);
            return new SupplierResponseDto
            {
                Id = s.Id,
                Name = s.Name,
                ContactPerson = s.ContactPerson,
                Phone = s.Phone,
                Email = s.Email,
                Address = s.Address,
                TaxCode = s.TaxCode,
                BankAccount = s.BankAccount,
                BankName = s.BankName,
                TotalDebt = s.TotalDebt,
                Note = s.Note,
                IsActive = s.IsActive
            };
        }

        public async Task<SupplierResponseDto> CreateAsync(CreateSupplierRequestDto request)
        {
            if (await _db.Suppliers.AnyAsync(s => s.Phone == request.Phone.Trim()))
                throw new DuplicateException($"Số điện thoại '{request.Phone}' đã được sử dụng");

            var supplier = new Supplier
            {
                Name = request.Name.Trim(),
                ContactPerson = request.ContactPerson?.Trim(),
                Phone = request.Phone.Trim(),
                Email = request.Email?.Trim(),
                Address = request.Address?.Trim(),
                TaxCode = request.TaxCode?.Trim(),
                BankAccount = request.BankAccount?.Trim(),
                BankName = request.BankName?.Trim(),
                Note = request.Note?.Trim(),
                TotalDebt = 0,
                IsActive = true
            };

            _db.Suppliers.Add(supplier);
            await _db.SaveChangesAsync();

            return await GetByIdAsync(supplier.Id);
        }

        public async Task<SupplierResponseDto> UpdateAsync(Guid id, CreateSupplierRequestDto request)
        {
            var supplier = await _db.Suppliers.FindAsync(id) ?? throw new NotFoundException("Nhà cung cấp", id);

            if (await _db.Suppliers.AnyAsync(s => s.Phone == request.Phone.Trim() && s.Id != id))
                throw new DuplicateException($"Số điện thoại '{request.Phone}' đã được sử dụng bởi NCC khác");

            supplier.Name = request.Name.Trim();
            supplier.ContactPerson = request.ContactPerson?.Trim();
            supplier.Phone = request.Phone.Trim();
            supplier.Email = request.Email?.Trim();
            supplier.Address = request.Address?.Trim();
            supplier.TaxCode = request.TaxCode?.Trim();
            supplier.BankAccount = request.BankAccount?.Trim();
            supplier.BankName = request.BankName?.Trim();
            supplier.Note = request.Note?.Trim();

            await _db.SaveChangesAsync();
            return await GetByIdAsync(supplier.Id);
        }

        public async Task ToggleActiveAsync(Guid id)
        {
            var supplier = await _db.Suppliers.FindAsync(id) ?? throw new NotFoundException("Nhà cung cấp", id);
            supplier.IsActive = !supplier.IsActive;
            await _db.SaveChangesAsync();
        }
    }
}