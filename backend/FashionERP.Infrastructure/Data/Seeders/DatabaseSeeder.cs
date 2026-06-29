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

            // 1. HR (cơ bản)
            await DepartmentSeeder.SeedAsync(db);
            await EmployeeSeeder.SeedAsync(db);
            await UserSeeder.SeedAsync(db);

            // 2. Catalog
            await CategoryBrandSeeder.SeedAsync(db);
            await ProductSeeder.SeedAsync(db);
            await VariantInventorySeeder.SeedAsync(db);

            // 3. Procurement — chạy SAU VariantInventorySeeder (cần Inventory có sẵn
            //    để cộng dồn AvgCost khi "nhận hàng"), TRƯỚC OrderSeeder để AvgCost
            //    ổn định trước khi OrderSeeder snapshot UnitCostSnapshot.
            await SupplierSeeder.SeedAsync(db);
            await PurchaseOrderSeeder.SeedAsync(db);

            // 4. Customers & Promotions
            await CustomerSeeder.SeedAsync(db);
            await PromotionSeeder.SeedAsync(db);
            await SizeChartSeeder.SeedAsync(db);

            // 5. Orders & sổ quỹ doanh thu (CashTransaction INCOME sinh kèm trong OrderSeeder)
            await OrderSeeder.SeedAsync(db);

            // 6. Chi phí vận hành (sổ quỹ EXPENSE đi kèm)
            await ExpenseSeeder.SeedAsync(db);

            // 7. HR vận hành — Attendance PHẢI chạy trước Payroll (Payroll tính từ Attendance)
            await AttendanceSeeder.SeedAsync(db);
            await LeaveSeeder.SeedAsync(db);
            await PayrollSeeder.SeedAsync(db);

            Console.WriteLine("========================================");
            Console.WriteLine("[Seeder] ✅ Hoàn tất seed dữ liệu mẫu!");
            Console.WriteLine("========================================");
        }
    }
}