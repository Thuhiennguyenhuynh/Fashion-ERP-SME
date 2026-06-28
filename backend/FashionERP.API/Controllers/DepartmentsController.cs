using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Domain.Entities;
using FashionERP.Infrastructure.Data;

namespace FashionERP.API.Controllers
{
    [Authorize(Roles = "Admin,Manager")]
    public class DepartmentsController : BaseController
    {
        private readonly AppDbContext _db;
        private readonly IMapper _mapper;

        public DepartmentsController(AppDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }

        public record CreateDeptDto(string Name, string? Description, Guid? ManagerId);
        public record DeptResponseDto(Guid Id, string Name, string? Description,
            string? ManagerName, int EmployeeCount);

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var list = await _db.Departments
                .Include(d => d.Manager)
                .Include(d => d.Employees)
                .Select(d => new DeptResponseDto(
                    d.Id, d.Name, d.Description,
                    d.Manager != null ? d.Manager.FullName : null,
                    d.Employees.Count))
                .ToListAsync();
            return Ok(list);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateDeptDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest("Tên phòng ban không được để trống");

            if (await _db.Departments.AnyAsync(d => d.Name == request.Name.Trim()))
                throw new DuplicateException($"Tên phòng ban '{request.Name}' đã tồn tại");

            if (request.ManagerId.HasValue &&
                !await _db.Employees.AnyAsync(e => e.Id == request.ManagerId.Value))
                throw new NotFoundException("Nhân viên quản lý", request.ManagerId.Value);

            var dept = new Department
            {
                Name = request.Name.Trim(),
                Description = request.Description?.Trim(),
                ManagerId = request.ManagerId
            };
            _db.Departments.Add(dept);
            await _db.SaveChangesAsync();

            return Created(new DeptResponseDto(dept.Id, dept.Name, dept.Description, null, 0),
                "Tạo phòng ban thành công");
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(Guid id, [FromBody] CreateDeptDto request)
        {
            var dept = await _db.Departments.FindAsync(id)
                ?? throw new NotFoundException("Phòng ban", id);

            if (await _db.Departments.AnyAsync(d => d.Name == request.Name.Trim() && d.Id != id))
                throw new DuplicateException($"Tên phòng ban '{request.Name}' đã tồn tại");

            dept.Name = request.Name.Trim();
            dept.Description = request.Description?.Trim();
            dept.ManagerId = request.ManagerId;
            dept.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok<object>(null!, "Cập nhật phòng ban thành công");
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var dept = await _db.Departments.FindAsync(id)
                ?? throw new NotFoundException("Phòng ban", id);

            if (await _db.Employees.AnyAsync(e => e.DepartmentId == id))
                throw new BusinessException("Không thể xóa phòng ban còn nhân viên. Hãy chuyển nhân viên sang phòng ban khác trước");

            _db.Departments.Remove(dept);
            await _db.SaveChangesAsync();
            return Ok<object>(null!, "Xóa phòng ban thành công");
        }
    }
}


