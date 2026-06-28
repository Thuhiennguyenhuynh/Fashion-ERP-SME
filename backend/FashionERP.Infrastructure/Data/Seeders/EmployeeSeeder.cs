namespace FashionERP.Infrastructure.Data.Seeders
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using FashionERP.Domain.Entities;
    using FashionERP.Domain.Enums;

    public static class EmployeeSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Employees.AnyAsync()) return;

            var employees = new List<Employee>
            {
                new()
                {
                    Id           = SeedIds.Emp_Admin,
                    FullName     = "Nguyễn Văn Admin",
                    Phone        = "0901000001",
                    Email        = "admin@fashionerp.vn",
                    Gender       = Gender.Male,
                    DateOfBirth  = new DateTime(1985, 3, 15),
                    Address      = "123 Nguyễn Huệ, Quận 1, TP.HCM",
                    DepartmentId = SeedIds.Dept_QuanLy,
                    Position     = "Giám đốc",
                    BaseSalary   = 30_000_000,
                    WorkingDaysPerMonth = 26,
                    StartDate    = new DateTime(2020, 1, 1),
                    Status       = EmployeeStatus.Active
                },
                new()
                {
                    Id           = SeedIds.Emp_Manager,
                    FullName     = "Trần Thị Manager",
                    Phone        = "0901000002",
                    Email        = "manager@fashionerp.vn",
                    Gender       = Gender.Female,
                    DateOfBirth  = new DateTime(1990, 7, 20),
                    Address      = "456 Lê Lợi, Quận 1, TP.HCM",
                    DepartmentId = SeedIds.Dept_BanHang,
                    Position     = "Trưởng phòng kinh doanh",
                    BaseSalary   = 20_000_000,
                    WorkingDaysPerMonth = 26,
                    StartDate    = new DateTime(2021, 3, 1),
                    Status       = EmployeeStatus.Active
                },
                new()
                {
                    Id           = SeedIds.Emp_Sales1,
                    FullName     = "Lê Văn Sales",
                    Phone        = "0901000003",
                    Email        = "sales1@fashionerp.vn",
                    Gender       = Gender.Male,
                    DateOfBirth  = new DateTime(1998, 11, 5),
                    Address      = "789 Hai Bà Trưng, Quận 3, TP.HCM",
                    DepartmentId = SeedIds.Dept_BanHang,
                    Position     = "Nhân viên bán hàng",
                    BaseSalary   = 8_000_000,
                    WorkingDaysPerMonth = 26,
                    StartDate    = new DateTime(2022, 6, 1),
                    Status       = EmployeeStatus.Active
                },
                new()
                {
                    Id           = SeedIds.Emp_Warehouse,
                    FullName     = "Phạm Thị Kho",
                    Phone        = "0901000004",
                    Email        = "warehouse@fashionerp.vn",
                    Gender       = Gender.Female,
                    DateOfBirth  = new DateTime(1995, 4, 12),
                    Address      = "321 Đinh Tiên Hoàng, Quận Bình Thạnh, TP.HCM",
                    DepartmentId = SeedIds.Dept_Kho,
                    Position     = "Nhân viên kho",
                    BaseSalary   = 7_500_000,
                    WorkingDaysPerMonth = 26,
                    StartDate    = new DateTime(2022, 9, 1),
                    Status       = EmployeeStatus.Active
                },
                new()
                {
                    Id           = SeedIds.Emp_Accountant,
                    FullName     = "Hoàng Văn Kế Toán",
                    Phone        = "0901000005",
                    Email        = "accountant@fashionerp.vn",
                    Gender       = Gender.Male,
                    DateOfBirth  = new DateTime(1993, 8, 25),
                    Address      = "654 Võ Văn Tần, Quận 3, TP.HCM",
                    DepartmentId = SeedIds.Dept_KeToan,
                    Position     = "Kế toán viên",
                    BaseSalary   = 10_000_000,
                    WorkingDaysPerMonth = 26,
                    StartDate    = new DateTime(2021, 10, 1),
                    Status       = EmployeeStatus.Active
                },
            };

            await db.Employees.AddRangeAsync(employees);
            await db.SaveChangesAsync();
            Console.WriteLine("[Seeder] Employees: OK");
        }
    }
}
