namespace FashionERP.Infrastructure.Auth
{
    using FashionERP.Application.Interfaces;
    using Microsoft.AspNetCore.Identity;

    public class PasswordHasher : IPasswordHasher
    {
        // WorkFactor 12 - cân bằng giữa an toàn và tốc độ
        private const int WorkFactor = 12;

        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
        }

        public bool Verify(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}


