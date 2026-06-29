namespace FashionERP.Infrastructure.Data.Seeders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using FashionERP.Domain.Entities;
    using FashionERP.Domain.Enums;

    /// <summary>Seed chấm công 30 ngày gần nhất cho toàn bộ nhân viên (bỏ Chủ nhật)</summary>
    public static class AttendanceSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Attendances.AnyAsync()) return;

            var employees = await db.Employees.Select(e => e.Id).ToListAsync();
            if (!employees.Any()) return;

            var random = new Random();
            var now = DateTime.UtcNow.Date;
            var list = new List<Attendance>();

            foreach (var empId in employees)
            {
                for (int d = 30; d >= 1; d--)
                {
                    var date = now.AddDays(-d);
                    if (date.DayOfWeek == DayOfWeek.Sunday) continue; // nghỉ CN

                    var roll = random.Next(100);
                    AttendanceType type;
                    DateTime? checkIn = date.AddHours(8);
                    DateTime? checkOut = date.AddHours(17);
                    decimal? totalHours = 8;
                    decimal overtime = 0;

                    if (roll < 5) { type = AttendanceType.Absent; checkIn = null; checkOut = null; totalHours = 0; }
                    else if (roll < 12) { type = AttendanceType.Late; checkIn = date.AddHours(8).AddMinutes(30); totalHours = 7.5m; }
                    else if (roll < 18) { type = AttendanceType.EarlyLeave; checkOut = date.AddHours(16); totalHours = 7m; }
                    else
                    {
                        type = AttendanceType.Normal;
                        if (roll > 90) { overtime = random.Next(1, 4); checkOut = date.AddHours(17).AddHours((double)overtime); totalHours = 8 + overtime; }
                    }

                    list.Add(new Attendance
                    {
                        EmployeeId = empId,
                        WorkDate = date,
                        CheckIn = checkIn,
                        CheckOut = checkOut,
                        TotalHours = totalHours,
                        OvertimeHours = overtime,
                        Type = type
                    });
                }
            }

            await db.Attendances.AddRangeAsync(list);
            await db.SaveChangesAsync();
            Console.WriteLine("[Seeder] Attendances: OK");
        }
    }
}
