namespace FashionERP.Infrastructure.Data.Seeders
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using FashionERP.Domain.Entities;

    public static class SupplierSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Suppliers.AnyAsync()) return;

            var suppliers = new List<Supplier>
            {
                new()
                {
                    Id = SeedIds.Sup_VinaTextile,
                    Name = "Công ty TNHH Vina Textile",
                    ContactPerson = "Nguyễn Thị Hoa",
                    Phone = "0909111222",
                    Email = "contact@vinatextile.vn",
                    Address = "Khu CN Tân Bình, TP.HCM",
                    TaxCode = "0301234567",
                    BankAccount = "0071001234567",
                    BankName = "Vietcombank",
                    IsActive = true,
                    TotalDebt = 0
                },
                new()
                {
                    Id = SeedIds.Sup_HanoiFabric,
                    Name = "Hanoi Fabric Co., Ltd",
                    ContactPerson = "Trần Văn Long",
                    Phone = "0909333444",
                    Email = "sales@hanoifabric.vn",
                    Address = "Cụm CN Sài Đồng, Hà Nội",
                    TaxCode = "0102345678",
                    BankAccount = "1903456789012",
                    BankName = "Techcombank",
                    IsActive = true,
                    TotalDebt = 0
                },
                new()
                {
                    Id = SeedIds.Sup_SaigonDenim,
                    Name = "Saigon Denim Supplier",
                    ContactPerson = "Lê Thị Mai",
                    Phone = "0909555666",
                    Email = "info@saigondenim.vn",
                    Address = "Quận Bình Tân, TP.HCM",
                    TaxCode = "0303456789",
                    BankAccount = "0451002345678",
                    BankName = "BIDV",
                    IsActive = true,
                    TotalDebt = 0
                }
            };

            await db.Suppliers.AddRangeAsync(suppliers);
            await db.SaveChangesAsync();
            Console.WriteLine("[Seeder] Suppliers: OK");
        }
    }
}
