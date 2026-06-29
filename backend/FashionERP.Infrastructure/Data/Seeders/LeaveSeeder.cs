namespace FashionERP.Infrastructure.Data.Seeders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using FashionERP.Domain.Entities;
    using FashionERP.Domain.Enums;

    public static class LeaveSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Leaves.AnyAsync()) return;

            var now = DateTime.UtcNow.Date;

            var leaves = new List<Leave>
            {
                new()
                {
                    EmployeeId = SeedIds.Emp_Sales1,
                    FromDate = now.AddDays(-20), ToDate = now.AddDays(-19), Days = 2,
                    Reason = "Việc gia đình", Status = LeaveStatus.Approved, ApprovedBy = SeedIds.Emp_Manager
                },
                new()
                {
                    EmployeeId = SeedIds.Emp_Warehouse,
                    FromDate = now.AddDays(-10), ToDate = now.AddDays(-10), Days = 1,
                    Reason = "Khám bệnh", Status = LeaveStatus.Approved, ApprovedBy = SeedIds.Emp_Manager
                },
                new()
                {
                    EmployeeId = SeedIds.Emp_Accountant,
                    FromDate = now.AddDays(3), ToDate = now.AddDays(4), Days = 2,
                    Reason = "Nghỉ phép năm", Status = LeaveStatus.Pending
                },
                new()
                {
                    EmployeeId = SeedIds.Emp_Sales1,
                    FromDate = now.AddDays(-35), ToDate = now.AddDays(-34), Days = 2,
                    Reason = "Có việc đột xuất, xin nghỉ gấp", Status = LeaveStatus.Rejected, ApprovedBy = SeedIds.Emp_Manager
                },
            };

            await db.Leaves.AddRangeAsync(leaves);
            await db.SaveChangesAsync();
            Console.WriteLine("[Seeder] Leaves: OK");
        }
    }
}
