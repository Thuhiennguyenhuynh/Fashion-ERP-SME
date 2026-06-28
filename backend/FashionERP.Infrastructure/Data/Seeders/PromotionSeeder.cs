namespace FashionERP.Infrastructure.Data.Seeders
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using FashionERP.Domain.Entities;
    using FashionERP.Domain.Enums;

    public static class PromotionSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Promotions.AnyAsync()) return;

            var promotions = new List<Promotion>
            {
                new()
                {
                    Id = SeedIds.Promo_Welcome,
                    Code = "WELCOME10",
                    Name = "Giảm 10% cho khách hàng mới",
                    Type = PromotionType.Percent,
                    DiscountValue = 10,
                    MaxDiscount = 50_000,
                    MinOrderValue = 200_000,
                    UsageLimit = 1000,
                    UsedCount = 0,
                    StartDate = DateTime.UtcNow.AddDays(-30),
                    EndDate = DateTime.UtcNow.AddYears(1),
                    IsActive = true
                },
                new()
                {
                    Id = SeedIds.Promo_Summer,
                    Code = "SUMMER50K",
                    Name = "Giảm 50.000đ cho đơn từ 500.000đ",
                    Type = PromotionType.FixedAmount,
                    DiscountValue = 50_000,
                    MinOrderValue = 500_000,
                    UsageLimit = 500,
                    UsedCount = 0,
                    StartDate = DateTime.UtcNow.AddDays(-10),
                    EndDate = DateTime.UtcNow.AddMonths(2),
                    IsActive = true
                },
            };

            await db.Promotions.AddRangeAsync(promotions);
            await db.SaveChangesAsync();
            Console.WriteLine("[Seeder] Promotions: OK");
        }
    }
}
