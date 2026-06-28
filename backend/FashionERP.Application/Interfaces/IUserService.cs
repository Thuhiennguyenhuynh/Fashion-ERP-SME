using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using FashionERP.Application.DTOs.Auth;

namespace FashionERP.Application.Interfaces
{
    /// <summary>
    /// Quản lý tài khoản hệ thống — chỉ Admin truy cập.
    /// Implementation: FashionERP.Infrastructure.Services.UserService
    /// </summary>
    public interface IUserService
    {
        Task<List<UserListItemDto>> GetAllAsync();
        Task<UserListItemDto> CreateAsync(CreateUserRequestDto request);
        Task<UserListItemDto> ToggleActiveAsync(Guid id);
    }
}