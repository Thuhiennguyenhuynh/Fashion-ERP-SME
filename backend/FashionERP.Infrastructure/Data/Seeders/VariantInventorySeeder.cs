using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FashionERP.Domain.Entities;
using FashionERP.Domain.Enums;

namespace FashionERP.Infrastructure.Data.Seeders
{
    public static class VariantInventorySeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (await db.ProductVariants.AnyAsync()) return;

            // ==========================================
            // 1. DỮ LIỆU FIX CỨNG (GIỮ NGUYÊN CỦA BẠN)
            // ==========================================
            var variants = new List<ProductVariant>
            {
                new() { Id = SeedIds.Var_AoThunTrang_M_White, ProductId = SeedIds.Prod_AoThunTrang, Size = ProductSize.M, Color = "Trắng", ColorHex = "#FFFFFF", Sku = "PROD-2025-0001-M-TRANG", Barcode = "8938512345671", IsActive = true },
                new() { Id = SeedIds.Var_AoThunTrang_L_White, ProductId = SeedIds.Prod_AoThunTrang, Size = ProductSize.L, Color = "Trắng", ColorHex = "#FFFFFF", Sku = "PROD-2025-0001-L-TRANG", Barcode = "8938512345672", IsActive = true },
                new() { Id = SeedIds.Var_AoThunDen_M_Black, ProductId = SeedIds.Prod_AoThunDen, Size = ProductSize.M, Color = "Đen", ColorHex = "#000000", Sku = "PROD-2025-0002-M-DEN", Barcode = "8938512345673", IsActive = true },
                new() { Id = SeedIds.Var_AoThunDen_L_Black, ProductId = SeedIds.Prod_AoThunDen, Size = ProductSize.L, Color = "Đen", ColorHex = "#000000", Sku = "PROD-2025-0002-L-DEN", Barcode = "8938512345674", IsActive = true },
                new() { Id = SeedIds.Var_QuanJean_30_Blue, ProductId = SeedIds.Prod_QuanJean, Size = ProductSize.M, Color = "Xanh đậm", ColorHex = "#1B3A6B", Sku = "PROD-2025-0003-30-XANH", Barcode = "8938512345675", IsActive = true },
                new() { Id = SeedIds.Var_QuanJean_32_Blue, ProductId = SeedIds.Prod_QuanJean, Size = ProductSize.L, Color = "Xanh đậm", ColorHex = "#1B3A6B", Sku = "PROD-2025-0003-32-XANH", Barcode = "8938512345676", IsActive = true },
                new() { Id = SeedIds.Var_AoSoMi_M_White, ProductId = SeedIds.Prod_AoSoMi, Size = ProductSize.M, Color = "Trắng", ColorHex = "#FFFFFF", Sku = "PROD-2025-0004-M-TRANG", Barcode = "8938512345677", IsActive = true },
                new() { Id = SeedIds.Var_AoDamNu_S_Red, ProductId = SeedIds.Prod_AoDamNu, Size = ProductSize.S, Color = "Đỏ hoa nhí", ColorHex = "#C0392B", Sku = "PROD-2025-0005-S-DOHOA", Barcode = "8938512345678", IsActive = true },
            };

            await db.ProductVariants.AddRangeAsync(variants);
            await db.SaveChangesAsync();

            var inventories = new List<Inventory>();
            foreach (var v in variants)
            {
                inventories.Add(new Inventory
                {
                    VariantId = v.Id,
                    Quantity = 50,
                    MinStock = 10,
                    MaxStock = 200,
                    Location = "Kho chính - Q1",
                    AvgCost = 100_000,
                    LastImportDate = DateTime.UtcNow
                });
            }

            await db.Inventories.AddRangeAsync(inventories);
            await db.SaveChangesAsync();

            // ==========================================
            // 2. DỮ LIỆU AUTO-GEN (SINH THÊM CHO AI)
            // ==========================================
            var productsNoVariants = await db.Products
                .Include(p => p.Variants)
                .Where(p => !p.Variants.Any())
                .ToListAsync();

            if (productsNoVariants.Any())
            {
                var random = new Random();
                var sizes = new[] { ProductSize.S, ProductSize.M, ProductSize.L, ProductSize.XL };
                var colors = new[] { ("Đen", "#000000"), ("Trắng", "#FFFFFF"), ("Be", "#F5F5DC"), ("Xanh", "#0000FF") };

                var autoVariants = new List<ProductVariant>();
                var autoInventories = new List<Inventory>();

                foreach (var p in productsNoVariants)
                {
                    int numVariants = random.Next(1, 4); // 1-3 biến thể/sản phẩm
                    for (int i = 0; i < numVariants; i++)
                    {
                        var size = sizes[random.Next(sizes.Length)];
                        var colorObj = colors[random.Next(colors.Length)];
                        var sku = $"{p.ProductCode}-{size}-{colorObj.Item1}".Replace(" ", "").ToUpper();

                        // Bỏ qua nếu SKU bị trùng khi quay random
                        if (autoVariants.Any(v => v.Sku == sku)) continue;

                        var variant = new ProductVariant
                        {
                            Id = Guid.NewGuid(),
                            ProductId = p.Id,
                            Size = size,
                            Color = colorObj.Item1,
                            ColorHex = colorObj.Item2,
                            Sku = sku,
                            Price = p.BasePrice + random.Next(0, 3) * 10000,
                            IsActive = true
                        };
                        autoVariants.Add(variant);

                        autoInventories.Add(new Inventory
                        {
                            VariantId = variant.Id,
                            Quantity = random.Next(10, 100),
                            MinStock = 10,
                            MaxStock = 200,
                            Location = "Kho tự động",
                            AvgCost = p.BasePrice * 0.6m,
                            LastImportDate = DateTime.UtcNow
                        });
                    }
                }

                await db.ProductVariants.AddRangeAsync(autoVariants);
                await db.Inventories.AddRangeAsync(autoInventories);
                await db.SaveChangesAsync();
            }

            Console.WriteLine("[Seeder] ProductVariants + Inventory: OK (Đã bao gồm Auto-gen)");
        }
    }
}