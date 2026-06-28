using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using FashionERP.Domain.Entities;

namespace FashionERP.Infrastructure.Data.Seeders
{
    public static class CategoryBrandSeeder
    {
        public static async Task SeedAsync(AppDbContext db)
        {
            if (!await db.Categories.AnyAsync())
            {
                var categories = new List<Category>
                {
                    new() { Id = SeedIds.Cat_AoNam,   Name = "Áo nam",   Slug = "ao-nam",   SortOrder = 1, IsActive = true },
                    new() { Id = SeedIds.Cat_QuanNam, Name = "Quần nam", Slug = "quan-nam", SortOrder = 2, IsActive = true },
                    new() { Id = SeedIds.Cat_AoNu,    Name = "Áo nữ",    Slug = "ao-nu",    SortOrder = 3, IsActive = true },
                    new() { Id = SeedIds.Cat_QuanNu,  Name = "Quần nữ",  Slug = "quan-nu",  SortOrder = 4, IsActive = true },
                    new() { Id = SeedIds.Cat_PhuKien, Name = "Phụ kiện", Slug = "phu-kien", SortOrder = 5, IsActive = true },
                    // Danh mục con của "Áo nam"
                    new() { Id = SeedIds.Cat_AoThun,  Name = "Áo thun",  Slug = "ao-thun",  SortOrder = 1, IsActive = true, ParentId = SeedIds.Cat_AoNam },
                    new() { Id = SeedIds.Cat_AoSo,    Name = "Áo sơ mi", Slug = "ao-so-mi", SortOrder = 2, IsActive = true, ParentId = SeedIds.Cat_AoNam },
                };
                await db.Categories.AddRangeAsync(categories);
                await db.SaveChangesAsync();
                System.Console.WriteLine("[Seeder] Categories: OK");
            }

            if (!await db.Brands.AnyAsync())
            {
                var brands = new List<Brand>
                {
                    new() { Id = SeedIds.Brand_Local,  Name = "FashionERP Local", Country = "Việt Nam" },
                    new() { Id = SeedIds.Brand_Nike,   Name = "Nike",             Country = "Mỹ" },
                    new() { Id = SeedIds.Brand_Adidas, Name = "Adidas",           Country = "Đức" },
                };
                await db.Brands.AddRangeAsync(brands);
                await db.SaveChangesAsync();
                System.Console.WriteLine("[Seeder] Brands: OK");
            }
        }
    }
}

