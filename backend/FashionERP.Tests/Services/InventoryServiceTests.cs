namespace FashionERP.Tests.Services
{
    using System;
    using System.Threading.Tasks;
    using AutoMapper;
    using Microsoft.EntityFrameworkCore;
    using FashionERP.Application.Common;
    using FashionERP.Application.DTOs.Inventory;
    using FashionERP.Application.Mappings;
    using FashionERP.Domain.Entities;
    using FashionERP.Domain.Enums;
    using FashionERP.Infrastructure.Data;
    using FashionERP.Infrastructure.Services;
    using FluentAssertions;
    using Xunit;
    using Moq;
    using Microsoft.AspNetCore.Http;

    public class InventoryServiceTests : IDisposable
    {
        private readonly AppDbContext _db;
        private readonly InventoryService _service;
        private readonly Guid _variantId = Guid.NewGuid();
        private readonly Guid _userId = Guid.NewGuid();

        public InventoryServiceTests()
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            var mockHttp = new Mock<IHttpContextAccessor>();
            _db = new AppDbContext(options, mockHttp.Object);
            _db.Database.EnsureCreated();

            var config = new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>());
            var mapper = config.CreateMapper();
            _service = new InventoryService(_db, mapper);

            var catId = Guid.NewGuid();
            _db.Categories.Add(new Category { Id = catId, Name = "Áo", Slug = "ao" });
            var prodId = Guid.NewGuid();
            _db.Products.Add(new Product
            {
                Id = prodId,
                Name = "Áo test",
                CategoryId = catId,
                Gender = ProductGender.Unisex,
                BasePrice = 100_000,
                ProductCode = "PROD-2025-9999",
                Status = ProductStatus.Active
            });
            _db.ProductVariants.Add(new ProductVariant
            {
                Id = _variantId,
                ProductId = prodId,
                Size = ProductSize.M,
                Color = "Trắng",
                Sku = "TEST-M-WHITE",
                IsActive = true
            });
            _db.Inventories.Add(new Inventory { VariantId = _variantId, Quantity = 10, MinStock = 5, AvgCost = 50_000 });
            _db.SaveChanges();
        }

        [Fact(DisplayName = "Nhập kho → tăng số lượng và cập nhật giá vốn trung bình")]
        public async Task ImportStockAsync_IncreasesQuantity_And_UpdatesAvgCost()
        {
            await _service.ImportStockAsync(new ImportStockRequestDto
            {
                VariantId = _variantId,
                Quantity = 10,
                UnitCost = 70_000,
                Note = "Nhập thêm hàng"
            }, _userId);

            var inv = await _db.Inventories.FirstAsync(i => i.VariantId == _variantId);
            inv.Quantity.Should().Be(20);
            inv.AvgCost.Should().Be(60_000);
        }

        [Fact(DisplayName = "Nhập kho → tạo bản ghi InventoryTransaction loại IMPORT")]
        public async Task ImportStockAsync_CreatesTransactionRecord()
        {
            await _service.ImportStockAsync(new ImportStockRequestDto
            {
                VariantId = _variantId,
                Quantity = 5,
                UnitCost = 60_000
            }, _userId);

            var txRecord = await _db.InventoryTransactions
                .FirstOrDefaultAsync(t => t.VariantId == _variantId);

            txRecord.Should().NotBeNull();
            txRecord!.Type.Should().Be(InventoryTransactionType.IMPORT);
            txRecord.QuantityBefore.Should().Be(10);
            txRecord.QuantityAfter.Should().Be(15);
        }

        [Fact(DisplayName = "Điều chỉnh tồn kho về cùng giá trị → ném BusinessException")]
        public async Task AdjustStockAsync_SameQuantity_ThrowsBusinessException()
        {
            var act = async () => await _service.AdjustStockAsync(new AdjustStockRequestDto
            {
                VariantId = _variantId,
                NewQuantity = 10
            }, _userId);

            await act.Should().ThrowAsync<BusinessException>();
        }

        [Fact(DisplayName = "Nhập kho cho variant không tồn tại → ném NotFoundException")]
        public async Task ImportStockAsync_VariantNotFound_ThrowsNotFoundException()
        {
            var act = async () => await _service.ImportStockAsync(new ImportStockRequestDto
            {
                VariantId = Guid.NewGuid(),
                Quantity = 5,
                UnitCost = 10_000
            }, _userId);

            await act.Should().ThrowAsync<NotFoundException>();
        }

        public void Dispose() => _db.Dispose();
    }
}