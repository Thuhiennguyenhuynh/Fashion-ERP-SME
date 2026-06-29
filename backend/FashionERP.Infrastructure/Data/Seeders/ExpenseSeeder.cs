namespace FashionERP.Infrastructure.Data.Seeders
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using FashionERP.Domain.Entities;
    using FashionERP.Domain.Enums;

    public static class ExpenseSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Expenses.AnyAsync()) return;

            var now = DateTime.UtcNow;
            var adminUserId = SeedIds.User_Admin;

            var expenses = new List<(string Category, decimal Amount, string Desc, int DaysAgo)>
            {
                ("Mặt bằng", 15_000_000, "Tiền thuê mặt bằng tháng này", 25),
                ("Điện nước", 2_500_000, "Hóa đơn điện nước tháng trước", 20),
                ("Marketing", 5_000_000, "Chạy ads Facebook/TikTok", 15),
                ("Mặt bằng", 15_000_000, "Tiền thuê mặt bằng tháng trước", 55),
                ("Vận chuyển", 1_200_000, "Phí ship hàng cho khách", 10),
                ("Văn phòng phẩm", 800_000, "Mua hóa đơn, bao bì đóng gói", 5),
            };

            var entities = new List<Expense>();

            foreach (var e in expenses)
            {
                var expense = new Expense
                {
                    Id = Guid.NewGuid(),
                    Category = e.Category,
                    Amount = e.Amount,
                    Description = e.Desc,
                    ExpenseDate = now.AddDays(-e.DaysAgo),
                };

                // ✅ FIX 1: set FK đúng cách (tránh shadow nếu có property thật)
                db.Entry(expense).Property("CreatorId").CurrentValue = adminUserId;

                entities.Add(expense);
            }

            await db.Expenses.AddRangeAsync(entities);
            await db.SaveChangesAsync();

            // ✅ FIX 2: gom CashTransaction lại (không SaveChanges trong loop)
            var cashTransactions = new List<CashTransaction>();

            foreach (var e in entities)
            {
                var lastBalance = await db.CashTransactions
                    .OrderByDescending(t => t.TransactionDate)
                    .ThenByDescending(t => t.CreatedAt)
                    .Select(t => t.BalanceAfter)
                    .FirstOrDefaultAsync();

                cashTransactions.Add(new CashTransaction
                {
                    Id = Guid.NewGuid(),
                    Type = CashTransactionType.EXPENSE,
                    Category = e.Category,
                    Amount = e.Amount,
                    Note = e.Description,
                    RefType = "Expense",
                    RefId = e.Id,
                    BalanceAfter = lastBalance - e.Amount,
                    TransactionDate = e.ExpenseDate,
                    CreatedAt = DateTime.UtcNow
                });
            }

            await db.CashTransactions.AddRangeAsync(cashTransactions);
            await db.SaveChangesAsync();

            Console.WriteLine("[Seeder] Expenses + Cash: OK");
        }
    }
}