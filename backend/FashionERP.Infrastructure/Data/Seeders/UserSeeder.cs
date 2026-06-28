namespace FashionERP.Infrastructure.Data.Seeders
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using FashionERP.Domain.Entities;
    using FashionERP.Domain.Enums;

    public static class UserSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Users.AnyAsync()) return;

            // Mật khẩu mặc định tất cả: Fashion@2025
            // BCrypt hash: work factor 12
            const string defaultHash =
                "$2a$12$KIp4y7S3o9OQXF7s.3GgXuP9wn4YhVqmGxT5f1WzQU8RXkMj4fWby";

            var users = new List<User>
            {
                new()
                {
                    Id         = SeedIds.User_Admin,
                    Email      = "admin@fashionerp.vn",
                    PasswordHash = defaultHash,
                    Role       = UserRole.Admin,
                    EmployeeId = SeedIds.Emp_Admin,
                    IsActive   = true
                },
                new()
                {
                    Id         = SeedIds.User_Manager,
                    Email      = "manager@fashionerp.vn",
                    PasswordHash = defaultHash,
                    Role       = UserRole.Manager,
                    EmployeeId = SeedIds.Emp_Manager,
                    IsActive   = true
                },
                new()
                {
                    Id         = SeedIds.User_Sales,
                    Email      = "sales1@fashionerp.vn",
                    PasswordHash = defaultHash,
                    Role       = UserRole.Sales,
                    EmployeeId = SeedIds.Emp_Sales1,
                    IsActive   = true
                },
                new()
                {
                    Id         = SeedIds.User_Warehouse,
                    Email      = "warehouse@fashionerp.vn",
                    PasswordHash = defaultHash,
                    Role       = UserRole.Warehouse,
                    EmployeeId = SeedIds.Emp_Warehouse,
                    IsActive   = true
                },
                new()
                {
                    Id         = SeedIds.User_Accountant,
                    Email      = "accountant@fashionerp.vn",
                    PasswordHash = defaultHash,
                    Role       = UserRole.Accountant,
                    EmployeeId = SeedIds.Emp_Accountant,
                    IsActive   = true
                },
            };

            await db.Users.AddRangeAsync(users);
            await db.SaveChangesAsync();
            Console.WriteLine("[Seeder] Users: OK");
            Console.WriteLine("[Seeder] ✅ Mật khẩu mặc định tất cả tài khoản: Fashion@2025");
        }
    }
}

