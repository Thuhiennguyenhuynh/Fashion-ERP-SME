namespace FashionERP.Application.Interfaces
{
    /// <summary>
    /// Hash và verify mật khẩu bằng BCrypt
    /// </summary>
    public interface IPasswordHasher
    {
        /// <summary>Hash mật khẩu thành chuỗi BCrypt (lưu vào User.PasswordHash)</summary>
        string Hash(string password);

        /// <summary>So khớp mật khẩu nhập vào với hash đã lưu</summary>
        bool Verify(string password, string hash);
    }
}

