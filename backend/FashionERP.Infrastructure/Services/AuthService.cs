using System;
using System.Threading.Tasks;
using AutoMapper;
using Microsoft.EntityFrameworkCore;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Auth;
using FashionERP.Application.Interfaces;
using FashionERP.Domain.Entities;
using FashionERP.Domain.Enums;
using FashionERP.Infrastructure.Data;

namespace FashionERP.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly IMapper _mapper;

        public AuthService(AppDbContext db, ITokenService tokenService,
            IPasswordHasher passwordHasher, IMapper mapper)
        {
            _db = db;
            _tokenService = tokenService;
            _passwordHasher = passwordHasher;
            _mapper = mapper;
        }

        // ─── LOGIN ────────────────────────────────────────────
        public async Task<AuthResponseDto> LoginAsync(LoginRequestDto request)
        {
            var user = await _db.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Email == request.Email.Trim().ToLower());

            if (user == null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
                throw new AppException("Email hoặc mật khẩu không chính xác", 401);

            if (!user.IsActive)
                throw new AppException("Tài khoản đã bị vô hiệu hóa. Vui lòng liên hệ quản trị viên", 403);

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = _tokenService.GenerateRefreshToken();

            // Lưu refresh token vào DB
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            user.LastLoginAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = _mapper.Map<UserInfoDto>(user)
            };
        }

        // ─── REFRESH TOKEN ────────────────────────────────────
        public async Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken)
                ?? throw new AppException("Access token không hợp lệ", 401);

            var userIdStr = principal.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                ?? throw new AppException("Token không chứa thông tin người dùng", 401);

            var userId = Guid.Parse(userIdStr);
            var user = await _db.Users
                .Include(u => u.Employee)
                .FirstOrDefaultAsync(u => u.Id == userId)
                ?? throw new NotFoundException("Người dùng", userId);

            if (user.RefreshToken != request.RefreshToken)
                throw new AppException("Refresh token không khớp", 401);

            if (user.RefreshTokenExpiry < DateTime.UtcNow)
                throw new AppException("Refresh token đã hết hạn. Vui lòng đăng nhập lại", 401);

            var newAccessToken = _tokenService.GenerateAccessToken(user);
            var newRefreshToken = _tokenService.GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                User = _mapper.Map<UserInfoDto>(user)
            };
        }

        // ─── CHANGE PASSWORD ──────────────────────────────────
        public async Task ChangePasswordAsync(Guid userId, ChangePasswordRequestDto request)
        {
            var user = await _db.Users.FindAsync(userId)
                ?? throw new NotFoundException("Người dùng", userId);

            if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
                throw new AppException("Mật khẩu hiện tại không chính xác", 400);

            user.PasswordHash = _passwordHasher.Hash(request.NewPassword);
            // Vô hiệu hóa refresh token cũ → buộc đăng nhập lại trên các thiết bị khác
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }

        // ─── CREATE USER ──────────────────────────────────────
        public async Task<UserInfoDto> CreateUserAsync(CreateUserRequestDto request)
        {
            var emailNorm = request.Email.Trim().ToLower();
            var exists = await _db.Users.AnyAsync(u => u.Email == emailNorm);
            if (exists)
                throw new DuplicateException($"Email '{request.Email}' đã được sử dụng bởi tài khoản khác");

            if (!Enum.TryParse<UserRole>(request.Role, out var role))
                throw new AppException("Vai trò không hợp lệ", 400);

            // Kiểm tra nhân viên tồn tại nếu có
            if (request.EmployeeId.HasValue)
            {
                var empExists = await _db.Employees.AnyAsync(e => e.Id == request.EmployeeId.Value);
                if (!empExists)
                    throw new NotFoundException("Nhân viên", request.EmployeeId.Value);

                // Mỗi nhân viên chỉ có 1 tài khoản
                var alreadyLinked = await _db.Users.AnyAsync(u => u.EmployeeId == request.EmployeeId.Value);
                if (alreadyLinked)
                    throw new DuplicateException("Nhân viên này đã có tài khoản đăng nhập");
            }

            var user = new User
            {
                Email = emailNorm,
                PasswordHash = _passwordHasher.Hash(request.Password),
                Role = role,
                EmployeeId = request.EmployeeId,
                IsActive = true
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            // Reload với navigation
            await _db.Entry(user).Reference(u => u.Employee).LoadAsync();
            return _mapper.Map<UserInfoDto>(user);
        }

        // ─── DEACTIVATE USER ──────────────────────────────────
        public async Task DeactivateUserAsync(Guid userId)
        {
            var user = await _db.Users.FindAsync(userId)
                ?? throw new NotFoundException("Người dùng", userId);

            user.IsActive = false;
            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
        // ─── LOGOUT ─────────────────────────────────────────────
        public async Task LogoutAsync(Guid userId)
        {
            var user = await _db.Users.FindAsync(userId)
                ?? throw new NotFoundException("Tài khoản", userId);

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;
            user.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        }
    }
}


