namespace FashionERP.Infrastructure.Data.Seeders
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using FashionERP.Domain.Entities;
    using FashionERP.Domain.Enums;

    public static class ProductSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.Products.AnyAsync()) return;

            var products = new List<Product>
            {
                new()
                {
                    Id = SeedIds.Prod_AoThunTrang,
                    Name = "Áo thun trơn cổ tròn",
                    Description = "Áo thun cotton 100%, form regular fit, thoáng mát",
                    CategoryId = SeedIds.Cat_AoThun,
                    BrandId = SeedIds.Brand_Local,
                    Gender = ProductGender.Male,
                    BasePrice = 199_000,
                    Tags = "basic,best-seller",
                    Status = ProductStatus.Active,
                    ProductCode = "PROD-2025-0001"
                },
                new()
                {
                    Id = SeedIds.Prod_AoThunDen,
                    Name = "Áo thun in họa tiết",
                    Description = "Áo thun cotton phối in họa tiết streetwear",
                    CategoryId = SeedIds.Cat_AoThun,
                    BrandId = SeedIds.Brand_Local,
                    Gender = ProductGender.Unisex,
                    BasePrice = 259_000,
                    Tags = "new,streetwear",
                    Status = ProductStatus.Active,
                    ProductCode = "PROD-2025-0002"
                },
                new()
                {
                    Id = SeedIds.Prod_QuanJean,
                    Name = "Quần jean slim fit",
                    Description = "Quần jean nam form slim, vải denim co giãn nhẹ",
                    CategoryId = SeedIds.Cat_QuanNam,
                    BrandId = SeedIds.Brand_Local,
                    Gender = ProductGender.Male,
                    BasePrice = 459_000,
                    Tags = "denim,best-seller",
                    Status = ProductStatus.Active,
                    ProductCode = "PROD-2025-0003"
                },
                new()
                {
                    Id = SeedIds.Prod_AoSoMi,
                    Name = "Áo sơ mi công sở",
                    Description = "Áo sơ mi dài tay vải kate Hàn Quốc, form slim fit",
                    CategoryId = SeedIds.Cat_AoSo,
                    BrandId = SeedIds.Brand_Local,
                    Gender = ProductGender.Male,
                    BasePrice = 329_000,
                    Tags = "office,formal",
                    Status = ProductStatus.Active,
                    ProductCode = "PROD-2025-0004"
                },
                new()
                {
                    Id = SeedIds.Prod_AoDamNu,
                    Name = "Đầm midi hoa nhí",
                    Description = "Đầm nữ midi tay ngắn họa tiết hoa nhí, vải voan",
                    CategoryId = SeedIds.Cat_AoNu,
                    BrandId = SeedIds.Brand_Local,
                    Gender = ProductGender.Female,
                    BasePrice = 389_000,
                    Tags = "new,summer",
                    Status = ProductStatus.Active,
                    ProductCode = "PROD-2025-0005"
                },


            };
            // -- ĐOẠN MỚI: SINH THÊM 45 SẢN PHẨM AUTO --
            var random = new Random();
            string[] types = { "Áo polo", "Quần short", "Áo khoác", "Chân váy", "Áo len", "Quần tây" };
            string[] styles = { "Basic", "Hàn Quốc", "Vintage", "Oversize", "Slimfit" };
            var cats = new[] { SeedIds.Cat_AoNam, SeedIds.Cat_AoNu, SeedIds.Cat_QuanNam, SeedIds.Cat_QuanNu };
            var genders = new[] { ProductGender.Male, ProductGender.Female, ProductGender.Unisex };

            for (int i = 6; i <= 55; i++)
            {
                var type = types[random.Next(types.Length)];
                var style = styles[random.Next(styles.Length)];
                var catId = cats[random.Next(cats.Length)];

                products.Add(new Product
                {
                    Id = Guid.NewGuid(), // Dùng Guid động cho dữ liệu sinh thêm
                    Name = $"{type} {style} tự động {i}",
                    Description = $"Sản phẩm {type} phong cách {style} chất lượng cao, đường may tỉ mỉ.",
                    CategoryId = catId,
                    BrandId = SeedIds.Brand_Local,
                    Gender = genders[random.Next(genders.Length)],
                    BasePrice = random.Next(15, 60) * 10000, // Giá từ 150k - 600k
                    Tags = "auto-gen",
                    Status = ProductStatus.Active,
                    ProductCode = $"PROD-2025-{i:D4}"
                });
            }

                await db.Products.AddRangeAsync(products);
            await db.SaveChangesAsync();
            System.Console.WriteLine("[Seeder] Products: OK");
        }
    }
}

