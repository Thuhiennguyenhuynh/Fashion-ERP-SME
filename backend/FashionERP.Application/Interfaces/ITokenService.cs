using System.Security.Claims;
using FashionERP.Domain.Entities;

namespace FashionERP.Application.Interfaces
{
    /// <summary>
    /// Sinh và xác thực JWT access token + refresh token
    /// </summary>
    public interface ITokenService
    {
        /// <summary>Sinh JWT access token (exp theo cấu hình, mặc định 15 phút)</summary>
        string GenerateAccessToken(User user);

        /// <summary>Sinh refresh token random 64 bytes (base64)</summary>
        string GenerateRefreshToken();

        /// <summary>Đọc claims từ access token đã hết hạn (dùng cho luồng refresh)</summary>
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
    }
}
