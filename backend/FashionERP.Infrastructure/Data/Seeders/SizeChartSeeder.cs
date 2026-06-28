namespace FashionERP.Infrastructure.Data.Seeders
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using FashionERP.Domain.Entities;
    using FashionERP.Domain.Enums;

    public static class SizeChartSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.SizeCharts.AnyAsync()) return;

            // Bảng size áo thun Nam (Shirt - Male) - dùng cho AI gợi ý size
            var charts = new List<SizeChart>
            {
                new() {
                    ProductType = SizeChartProductType.Shirt, Gender = SizeChartGender.Male, Size = SizeChartSize.S,
                    MinHeight = 155, MaxHeight = 165, MinWeight = 45, MaxWeight = 55,
                    MinChest = 84, MaxChest = 88, MinWaist = 70, MaxWaist = 74
                },
                new() {
                    ProductType = SizeChartProductType.Shirt, Gender = SizeChartGender.Male, Size = SizeChartSize.M,
                    MinHeight = 163, MaxHeight = 172, MinWeight = 53, MaxWeight = 63,
                    MinChest = 88, MaxChest = 92, MinWaist = 74, MaxWaist = 78
                },
                new() {
                    ProductType = SizeChartProductType.Shirt, Gender = SizeChartGender.Male, Size = SizeChartSize.L,
                    MinHeight = 170, MaxHeight = 178, MinWeight = 61, MaxWeight = 71,
                    MinChest = 92, MaxChest = 96, MinWaist = 78, MaxWaist = 82
                },
                new() {
                    ProductType = SizeChartProductType.Shirt, Gender = SizeChartGender.Male, Size = SizeChartSize.XL,
                    MinHeight = 175, MaxHeight = 183, MinWeight = 69, MaxWeight = 80,
                    MinChest = 96, MaxChest = 102, MinWaist = 82, MaxWaist = 88
                },
                // Bảng size áo Nữ (Shirt - Female)
                new() {
                    ProductType = SizeChartProductType.Shirt, Gender = SizeChartGender.Female, Size = SizeChartSize.S,
                    MinHeight = 150, MaxHeight = 158, MinWeight = 40, MaxWeight = 48,
                    MinChest = 80, MaxChest = 84, MinWaist = 62, MaxWaist = 66
                },
                new() {
                    ProductType = SizeChartProductType.Shirt, Gender = SizeChartGender.Female, Size = SizeChartSize.M,
                    MinHeight = 156, MaxHeight = 163, MinWeight = 46, MaxWeight = 54,
                    MinChest = 84, MaxChest = 88, MinWaist = 66, MaxWaist = 70
                },
                new() {
                    ProductType = SizeChartProductType.Shirt, Gender = SizeChartGender.Female, Size = SizeChartSize.L,
                    MinHeight = 160, MaxHeight = 168, MinWeight = 52, MaxWeight = 60,
                    MinChest = 88, MaxChest = 94, MinWaist = 70, MaxWaist = 76
                },
            };

            await db.SizeCharts.AddRangeAsync(charts);
            await db.SaveChangesAsync();
            System.Console.WriteLine("[Seeder] SizeCharts: OK");
        }
    }
}
