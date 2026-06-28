using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FashionERP.Domain.Entities;
using FashionERP.Domain.Enums;

namespace FashionERP.Infrastructure.Data.Seeders
{
    public static class CustomerSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Customers.AnyAsync()) return;

            var customers = new List<Customer>
            {
                new()
                {
                    Id = SeedIds.Cust_1, FullName = "Nguyễn Thị Khách Hàng",
                    Phone = "0912345671", Email = "khach1@gmail.com",
                    Gender = Gender.Female, DateOfBirth = new DateTime(1996, 5, 10),
                    Address = "12 Lý Tự Trọng, Quận 1, TP.HCM",
                    MemberLevel = MemberLevel.Silver, TotalSpent = 6_500_000, TotalOrders = 4
                },
                new()
                {
                    Id = SeedIds.Cust_2, FullName = "Trần Văn Mua Sắm",
                    Phone = "0912345672", Email = "khach2@gmail.com",
                    Gender = Gender.Male, DateOfBirth = new DateTime(1992, 9, 22),
                    Address = "34 Trần Hưng Đạo, Quận 5, TP.HCM",
                    MemberLevel = MemberLevel.Bronze, TotalSpent = 1_200_000, TotalOrders = 1
                },
                new()
                {
                    Id = SeedIds.Cust_3, FullName = "Phạm Thị VIP",
                    Phone = "0912345673", Email = "khach3@gmail.com",
                    Gender = Gender.Female, DateOfBirth = new DateTime(1988, 1, 30),
                    Address = "56 Nguyễn Trãi, Quận 5, TP.HCM",
                    MemberLevel = MemberLevel.Gold, TotalSpent = 22_000_000, TotalOrders = 15
                },
            };

            await db.Customers.AddRangeAsync(customers);
            await db.SaveChangesAsync();
            Console.WriteLine("[Seeder] Customers: OK");
        }
    }
}
