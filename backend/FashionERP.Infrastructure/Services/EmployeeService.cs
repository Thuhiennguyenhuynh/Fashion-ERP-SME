using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Employee;
using FashionERP.Application.Interfaces;
using FashionERP.Domain.Entities;
using FashionERP.Domain.Enums;
using FashionERP.Infrastructure.Data;

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
        public async Task<List<EmployeeResponseDto>> GetAllAsync()
        {
            var list = await BaseQuery()
                .OrderBy(e => e.FullName)
                .ToListAsync();
            return _mapper.Map<List<EmployeeResponseDto>>(list);
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
            // Kiểm tra số điện thoại trùng
            if (await _db.Employees.AnyAsync(e => e.Phone == request.Phone))
                throw new DuplicateException($"Số điện thoại '{request.Phone}' đã được sử dụng bởi nhân viên khác");

            // Kiểm tra email trùng (nếu có)
            if (!string.IsNullOrEmpty(request.Email) &&
                await _db.Employees.AnyAsync(e => e.Email == request.Email.Trim().ToLower()))
                throw new DuplicateException($"Email '{request.Email}' đã được sử dụng bởi nhân viên khác");

            // Kiểm tra phòng ban tồn tại
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

            // Kiểm tra số điện thoại trùng với nhân viên KHÁC
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
            var emp = await _db.Employees.FindAsync(id)
                ?? throw new NotFoundException("Nhân viên", id);

            if (await _db.Orders.AnyAsync(o => o.StaffId == id))
            {
                // Đã có đơn hàng -> Soft Delete bằng cách chuyển trạng thái sang Nghỉ việc (Resigned)
                emp.Status = EmployeeStatus.Resigned;
                _db.Employees.Update(emp);
            }
            else
            {
                // Chưa có dữ liệu liên kết -> Hard Delete (Xóa thật)
                _db.Employees.Remove(emp);
            }

            await _db.SaveChangesAsync();
        }


        public async Task UpdateStatusAsync(Guid id, string status)
        {
            var emp = await _db.Employees.FindAsync(id)
                ?? throw new NotFoundException("Nhân viên", id);
            emp.Status = status;
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


