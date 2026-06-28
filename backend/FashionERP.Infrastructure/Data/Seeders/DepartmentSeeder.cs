using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FashionERP.Domain.Entities;

namespace FashionERP.Infrastructure.Data.Seeders
{
    public static class DepartmentSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Departments.AnyAsync()) return;

            var departments = new List<Department>
            {
                new() { Id = SeedIds.Dept_BanHang,   Name = "Bán hàng",   Description = "Phòng kinh doanh và bán hàng trực tiếp" },
                new() { Id = SeedIds.Dept_Kho,        Name = "Kho",         Description = "Quản lý nhập xuất tồn kho" },
                new() { Id = SeedIds.Dept_KeToan,     Name = "Kế toán",     Description = "Quản lý tài chính và lương thưởng" },
                new() { Id = SeedIds.Dept_Marketing,  Name = "Marketing",   Description = "Phòng marketing và truyền thông" },
                new() { Id = SeedIds.Dept_QuanLy,     Name = "Quản lý",     Description = "Ban lãnh đạo và quản lý chung" },
            };

            await db.Departments.AddRangeAsync(departments);
            await db.SaveChangesAsync();
            Console.WriteLine("[Seeder] Departments: OK");
        }
    }
}
