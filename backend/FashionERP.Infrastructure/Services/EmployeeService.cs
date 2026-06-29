using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Employee;
using FashionERP.Application.Interfaces;
using FashionERP.Domain.Entities;
using FashionERP.Domain.Enums;
using FashionERP.Infrastructure.Data;
using System.Linq.Expressions;

namespace FashionERP.Infrastructure.Services
{
    public class EmployeeService : IEmployeeService
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public EmployeeService(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        private IQueryable<Employee> BaseQuery() =>
            _db.Employees.Include(e => e.Department);

        // ─── GET ALL ──────────────────────────────────────────
        public async Task<PagedResult<EmployeeResponseDto>> GetAllAsync(EmployeeQueryParams p)
        {
            var query = _db.Employees
                .Where(e => !e.IsDeleted)
                .Include(e => e.Department)
                .AsQueryable();

            // Smart search: tên, phone, email, chức vụ
            query = query.SmartSearch(p.Keyword,
                e => e.FullName,
                e => e.Phone,
                e => e.Email,
                e => e.Position);

            // Filter phòng ban
            if (p.DepartmentId.HasValue)
                query = query.Where(e => e.DepartmentId == p.DepartmentId.Value);

            // Filter trạng thái
            if (!string.IsNullOrEmpty(p.Status) &&
                Enum.TryParse<EmployeeStatus>(p.Status, out var status))
                query = query.Where(e => e.Status == status);

            // Filter chức vụ
            if (!string.IsNullOrEmpty(p.Position))
                query = query.Where(e => e.Position.Contains(p.Position));

            // Filter lương
            if (p.MinSalary.HasValue)
                query = query.Where(e => e.BaseSalary >= p.MinSalary.Value);
            if (p.MaxSalary.HasValue)
                query = query.Where(e => e.BaseSalary <= p.MaxSalary.Value);

            // Sort
            var sortMap = new Dictionary<string, Expression<Func<Employee, object>>>
            {
                ["fullname"] = e => e.FullName,
                ["basesalary"] = e => e.BaseSalary,
                ["startdate"] = e => e.StartDate,
                ["status"] = e => e.Status,
                ["createdat"] = e => e.CreatedAt,
            };
            query = query.ApplySort(p.SortBy, sortMap, "fullname");

            var paged = await query.ToPagedResultAsync(p.Page, p.PageSize);
            return paged.MapTo(_mapper.Map<List<EmployeeResponseDto>>);
        }

        // ─── GET BY ID ────────────────────────────────────────
        public async Task<EmployeeResponseDto> GetByIdAsync(Guid id)
        {
            var emp = await BaseQuery().FirstOrDefaultAsync(e => e.Id == id)
                ?? throw new NotFoundException("Nhân viên", id);
            return _mapper.Map<EmployeeResponseDto>(emp);
        }

        // ─── CREATE ───────────────────────────────────────────
        public async Task<EmployeeResponseDto> CreateAsync(CreateEmployeeRequestDto request)
        {
            if (await _db.Employees.AnyAsync(e => e.Phone == request.Phone))
                throw new DuplicateException($"Số điện thoại '{request.Phone}' đã được sử dụng bởi nhân viên khác");

            if (!string.IsNullOrEmpty(request.Email) &&
                await _db.Employees.AnyAsync(e => e.Email == request.Email.Trim().ToLower()))
                throw new DuplicateException($"Email '{request.Email}' đã được sử dụng bởi nhân viên khác");

            if (!await _db.Departments.AnyAsync(d => d.Id == request.DepartmentId))
                throw new NotFoundException("Phòng ban", request.DepartmentId);

            Gender? gender = null;
            if (!string.IsNullOrEmpty(request.Gender) && Enum.TryParse<Gender>(request.Gender, out var g))
                gender = g;

            var emp = new Employee
            {
                FullName = request.FullName.Trim(),
                Phone = request.Phone.Trim(),
                Email = request.Email?.Trim().ToLower(),
                Gender = gender,
                DateOfBirth = request.DateOfBirth,
                Address = request.Address?.Trim(),
                DepartmentId = request.DepartmentId,
                Position = request.Position.Trim(),
                BaseSalary = request.BaseSalary,
                WorkingDaysPerMonth = request.WorkingDaysPerMonth,
                StartDate = request.StartDate,
                Status = EmployeeStatus.Probation
            };

            _db.Employees.Add(emp);
            await _db.SaveChangesAsync();

            await _db.Entry(emp).Reference(e => e.Department).LoadAsync();
            return _mapper.Map<EmployeeResponseDto>(emp);
        }

        // ─── UPDATE ───────────────────────────────────────────
        public async Task<EmployeeResponseDto> UpdateAsync(Guid id, UpdateEmployeeRequestDto request)
        {
            var emp = await BaseQuery().FirstOrDefaultAsync(e => e.Id == id)
                ?? throw new NotFoundException("Nhân viên", id);

            if (await _db.Employees.AnyAsync(e => e.Phone == request.Phone && e.Id != id))
                throw new DuplicateException($"Số điện thoại '{request.Phone}' đã được sử dụng bởi nhân viên khác");

            if (!string.IsNullOrEmpty(request.Email) &&
                await _db.Employees.AnyAsync(e => e.Email == request.Email.Trim().ToLower() && e.Id != id))
                throw new DuplicateException($"Email '{request.Email}' đã được sử dụng bởi nhân viên khác");

            if (!await _db.Departments.AnyAsync(d => d.Id == request.DepartmentId))
                throw new NotFoundException("Phòng ban", request.DepartmentId);

            Gender? gender = null;
            if (!string.IsNullOrEmpty(request.Gender) && Enum.TryParse<Gender>(request.Gender, out var g))
                gender = g;

            if (!Enum.TryParse<EmployeeStatus>(request.Status, out var status))
                throw new AppException("Trạng thái nhân viên không hợp lệ");

            emp.FullName = request.FullName.Trim();
            emp.Phone = request.Phone.Trim();
            emp.Email = request.Email?.Trim().ToLower();
            emp.Gender = gender;
            emp.DateOfBirth = request.DateOfBirth;
            emp.Address = request.Address?.Trim();
            emp.DepartmentId = request.DepartmentId;
            emp.Position = request.Position.Trim();
            emp.BaseSalary = request.BaseSalary;
            emp.WorkingDaysPerMonth = request.WorkingDaysPerMonth;
            emp.Status = status;
            emp.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return _mapper.Map<EmployeeResponseDto>(emp);
        }

        // ─── DELETE ───────────────────────────────────────────
        public async Task DeleteAsync(Guid id)
        {
            var emp = await _db.Employees
                .Include(e => e.Orders)
                .FirstOrDefaultAsync(e => e.Id == id)
                ?? throw new NotFoundException("Nhân viên", id);

            if (emp.Orders.Any())
                throw new BusinessException(
                    "Không thể xóa nhân viên đã có đơn hàng. " +
                    "Hãy chuyển trạng thái sang Resigned thay vì xóa.");

            // Gọi Remove() — SaveChangesAsync override tự convert sang soft delete
            // vì Employee giờ implement ISoftDeletable
            _db.Employees.Remove(emp);
            await _db.SaveChangesAsync();
        }

        // ─── UPDATE STATUS ────────────────────────────────────
        public async Task UpdateStatusAsync(Guid id, string status)
        {
            var emp = await _db.Employees.FindAsync(id)
                ?? throw new NotFoundException("Nhân viên", id);

            // SỬA Ở ĐÂY: Ép kiểu từ String sang Enum
            emp.Status = Enum.Parse<EmployeeStatus>(status);
            emp.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        // ─── UPDATE AVATAR ────────────────────────────────────
        public async Task UpdateAvatarAsync(Guid id, string imageUrl, string publicId)
        {
            var emp = await _db.Employees.FindAsync(id)
                ?? throw new NotFoundException("Nhân viên", id);

            emp.AvatarUrl = imageUrl;
            emp.AvatarPublicId = publicId;
            emp.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}