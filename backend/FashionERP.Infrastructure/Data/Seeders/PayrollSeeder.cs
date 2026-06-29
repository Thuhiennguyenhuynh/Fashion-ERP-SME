namespace FashionERP.Infrastructure.Data.Seeders
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using FashionERP.Domain.Entities;
    using FashionERP.Domain.Enums;

    /// <summary>
    /// Seed bảng lương tháng trước (Paid) cho toàn bộ nhân viên, dựa theo Attendance
    /// đã seed (yêu cầu AttendanceSeeder chạy trước). Mỗi payroll Paid sẽ tự sinh
    /// CashTransaction EXPENSE tương ứng để sổ quỹ đồng bộ.
    /// </summary>
    public static class PayrollSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Payrolls.AnyAsync()) return;

            var employees = await db.Employees.ToListAsync();
            if (!employees.Any()) return;

            var now = DateTime.UtcNow;
            var lastMonthDate = now.AddMonths(-1);
            int month = lastMonthDate.Month, year = lastMonthDate.Year;

            foreach (var emp in employees)
            {
                var attendances = await db.Attendances
                    .Where(a => a.EmployeeId == emp.Id && a.WorkDate.Month == month && a.WorkDate.Year == year)
                    .ToListAsync();

                decimal workingDaysActual = attendances.Count(a => a.Type == AttendanceType.Normal || a.Type == AttendanceType.Late);
                decimal overtimeHours = attendances.Sum(a => a.OvertimeHours);

                decimal hourlyRate = emp.BaseSalary / Math.Max(emp.WorkingDaysPerMonth, 1) / 8m;
                decimal overtimePay = overtimeHours * hourlyRate * 1.5m;
                decimal allowance = 500_000; // phụ cấp ăn trưa/xăng xe cố định cho data mẫu
                decimal deduction = 0;

                decimal netSalary = (emp.BaseSalary / Math.Max(emp.WorkingDaysPerMonth, 1) * workingDaysActual)
                                   + allowance + overtimePay - deduction;

                var payroll = new Payroll
                {
                    EmployeeId = emp.Id,
                    Month = month,
                    Year = year,
                    WorkingDaysActual = workingDaysActual,
                    BaseSalary = emp.BaseSalary,
                    Allowance = allowance,
                    OvertimePay = overtimePay,
                    Deduction = deduction,
                    NetSalary = Math.Round(netSalary, 0),
                    Status = PayrollStatus.Paid
                };

                db.Payrolls.Add(payroll);
                await db.SaveChangesAsync();

                // Đồng bộ sổ quỹ: trả lương = chi quỹ
                var lastBalance = await db.CashTransactions
                    .OrderByDescending(t => t.TransactionDate)
                    .ThenByDescending(t => t.CreatedAt)
                    .Select(t => t.BalanceAfter)
                    .FirstOrDefaultAsync();

                db.CashTransactions.Add(new CashTransaction
                {
                    Type = Domain.Enums.CashTransactionType.EXPENSE,
                    Category = "Lương",
                    Amount = payroll.NetSalary,
                    Note = $"Trả lương {emp.FullName} - {month}/{year}",
                    RefType = "Payroll",
                    RefId = payroll.Id,
                    BalanceAfter = lastBalance - payroll.NetSalary,
                    TransactionDate = new DateTime(year, month, 28)
                });
                await db.SaveChangesAsync();
            }

            Console.WriteLine("[Seeder] Payrolls: OK");
        }
    }
}
