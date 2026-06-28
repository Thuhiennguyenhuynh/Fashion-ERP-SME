using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Auth;
using FashionERP.Application.Interfaces;
using FashionERP.Domain.Entities;
using FashionERP.Infrastructure.Data;

namespace FashionERP.Infrastructure.Services
{
    public class UserService : IUserService
    {
        private static readonly string[] ValidRoles =
            ["Admin", "Manager", "Sales", "Warehouse", "Accountant"];

        private readonly AppDbContext _db;
        private readonly IPasswordHasher _hasher;

        public UserService(AppDbContext db, IPasswordHasher hasher)
        {
            _db = db;
            _hasher = hasher;
        }

        // ─── GET ALL ──────────────────────────────────────────────────────────
        public async Task<List<UserListItemDto>> GetAllAsync()
        {
            return await _db.Users
                .OrderBy(u => u.Email)
                .Select(u => new UserListItemDto
                {
                    Id = u.Id,
                    Email = u.Email,
                    Role = u.Role,
                    EmployeeId = u.EmployeeId,
                    IsActive = u.IsActive,
                    LastLoginAt = u.LastLoginAt
                })
                .ToListAsync();
        }

        // ─── CREATE ───────────────────────────────────────────────────────────
        public async Task<UserListItemDto> CreateAsync(CreateUserRequestDto request)
        {
            if (!Array.Exists(ValidRoles, r => r == request.Role))
                throw new BusinessException($"Role '{request.Role}' không hợp lệ. " +
                    "Phải là Admin, Manager, Sales, Warehouse hoặc Accountant.");

            var email = request.Email.Trim().ToLower();

            if (await _db.Users.AnyAsync(u => u.Email == email))
                throw new BusinessException("Email đã tồn tại trong hệ thống.");

            var user = new User
            {
                Email = email,
                PasswordHash = _hasher.Hash(request.Password),
                Role = request.Role,
                EmployeeId = request.EmployeeId,
                IsActive = true
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return new UserListItemDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                EmployeeId = user.EmployeeId,
                IsActive = user.IsActive
            };
        }

        // ─── TOGGLE ACTIVE ────────────────────────────────────────────────────
        public async Task<UserListItemDto> ToggleActiveAsync(Guid id)
        {
            var user = await _db.Users.FindAsync(id)
                ?? throw new NotFoundException("Tài khoản", id);

            user.IsActive = !user.IsActive;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new UserListItemDto
            {
                Id = user.Id,
                Email = user.Email,
                Role = user.Role,
                EmployeeId = user.EmployeeId,
                IsActive = user.IsActive,
                LastLoginAt = user.LastLoginAt
            };
        }
    }
}