namespace FashionERP.Infrastructure.Auth
{
    /// <summary>
    /// Cấu hình JWT - bind từ appsettings.json (section "Jwt")
    /// </summary>
    public class JwtSettings
    {
        public string SecretKey { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;

        /// <summary>Access token hết hạn sau X phút (mặc định 15)</summary>
        public int AccessTokenExpiryMinutes { get; set; } = 15;

        /// <summary>Refresh token hết hạn sau X ngày (mặc định 7)</summary>
        public int RefreshTokenExpiryDays { get; set; } = 7;
    }
}


