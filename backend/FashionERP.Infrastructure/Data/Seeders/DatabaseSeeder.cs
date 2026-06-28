namespace FashionERP.Infrastructure.Data.Seeders
{
    using System;
    using System.Threading.Tasks;

    public static class DatabaseSeeder
    {
        public static async Task SeedAllAsync(AppDbContext db)
        {
            Console.WriteLine("========================================");
            Console.WriteLine("[Seeder] Bắt đầu seed dữ liệu mẫu...");
            Console.WriteLine("========================================");

            // 1. HR
            await DepartmentSeeder.SeedAsync(db);
            await EmployeeSeeder.SeedAsync(db);
            await UserSeeder.SeedAsync(db);

            // 2. Catalog
            await CategoryBrandSeeder.SeedAsync(db);
            await ProductSeeder.SeedAsync(db);
            await VariantInventorySeeder.SeedAsync(db);

            // 3. Customers & Promotions
            await CustomerSeeder.SeedAsync(db);
            await PromotionSeeder.SeedAsync(db);
            await SizeChartSeeder.SeedAsync(db);

            // 4. Sinh Orders & Transactions (Cho AI)
            await OrderSeeder.SeedAsync(db);

            Console.WriteLine("========================================");
            Console.WriteLine("[Seeder] ✅ Hoàn tất seed dữ liệu mẫu!");
            Console.WriteLine("========================================");
        }
    }
}