using System.Threading.Tasks;
using FashionERP.Application.DTOs.Auth;

namespace FashionERP.Application.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequestDto request);
        Task<AuthResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request);
        Task ChangePasswordAsync(System.Guid userId, ChangePasswordRequestDto request);
        Task<UserInfoDto> CreateUserAsync(CreateUserRequestDto request);
        Task DeactivateUserAsync(System.Guid userId);
        Task LogoutAsync(Guid userId);
    
}
}
