namespace FashionERP.Tests.Services
{
    using System;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using FashionERP.Application.Common;
    using FashionERP.Application.DTOs.AI;
    using FashionERP.Application.Interfaces;
    using FashionERP.Domain.Entities;
    using FashionERP.Domain.Enums;
    using FashionERP.Infrastructure.Data;
    using FashionERP.Infrastructure.Services;
    using FluentAssertions;
    using Moq;
    using Xunit;
    using Microsoft.AspNetCore.Http;

    public class AIServiceTests : IDisposable
    {
        private readonly AppDbContext _db;
        private readonly Mock<IAIServiceClient> _aiClientMock;
        private readonly AIService _service;

        private readonly Guid _variantId = Guid.NewGuid();
        private readonly Guid _userId = Guid.NewGuid();

        public AIServiceTests()
        {
            var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
            connection.Open();
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(connection)
                .Options;

            // Mock HttpContextAccessor
            var mockHttp = new Mock<IHttpContextAccessor>();
            _db = new AppDbContext(options, mockHttp.Object);
            _db.Database.EnsureCreated();

            _aiClientMock = new Mock<IAIServiceClient>();
            _service = new AIService(_db, _aiClientMock.Object);

            _db.SizeCharts.Add(new SizeChart
            {
                Id = Guid.NewGuid(),
                ProductType = SizeChartProductType.Shirt,
                Gender = SizeChartGender.Male,
                Size = SizeChartSize.M,
                MinHeight = 160,
                MaxHeight = 170,
                MinWeight = 55,
                MaxWeight = 65
            });

            var catId = Guid.NewGuid();
            _db.Categories.Add(new Category { Id = catId, Name = "Áo", Slug = "ao" });

            var prodId = Guid.NewGuid();
            _db.Products.Add(new Product
            {
                Id = prodId,
                Name = "Áo test AI",
                CategoryId = catId,
                Gender = ProductGender.Unisex,
                BasePrice = 200_000,
                ProductCode = "PROD-2025-AI01",
                Status = ProductStatus.Active
            });

            _db.ProductVariants.Add(new ProductVariant
            {
                Id = _variantId,
                ProductId = prodId,
                Size = ProductSize.M,
                Color = "Đen",
                Sku = "AI-TEST-M-BLACK",
                IsActive = true
            });

            _db.Inventories.Add(new Inventory
            {
                VariantId = _variantId,
                Quantity = 20,
                MinStock = 5,
                AvgCost = 80_000
            });

            _db.SaveChanges();
        }

        [Fact(DisplayName = "Chatbot: gọi thành công → trả về reply và ghi log AILogs với IsSuccess=true")]
        public async Task ChatAsync_Success_LogsAILog()
        {
            _aiClientMock
                .Setup(c => c.ChatAsync(It.IsAny<AIChatbotProxyRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new ChatbotResponseDto { Reply = "Dạ shop có áo sơ mi giá 200k ạ" });

            var result = await _service.ChatAsync(new ChatbotRequestDto { Message = "Có áo sơ mi không?" }, _userId);

            result.Reply.Should().Contain("áo sơ mi");

            var log = await _db.AILogs.FirstOrDefaultAsync(l => l.Feature == AIFeature.Chatbot);
            log.Should().NotBeNull();
            log!.IsSuccess.Should().BeTrue();
        }

        [Fact(DisplayName = "Chatbot: message rỗng → ném AppException, không gọi sang AI client")]
        public async Task ChatAsync_EmptyMessage_ThrowsAppException()
        {
            var act = async () => await _service.ChatAsync(new ChatbotRequestDto { Message = "  " }, _userId);

            await act.Should().ThrowAsync<AppException>();
            _aiClientMock.Verify(
                c => c.ChatAsync(It.IsAny<AIChatbotProxyRequest>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact(DisplayName = "SizeRecommend: không có SizeChart phù hợp → ném AppException 404")]
        public async Task RecommendSizeAsync_NoMatchingSizeChart_Throws404()
        {
            var act = async () => await _service.RecommendSizeAsync(new SizeRecommendRequestDto
            {
                ProductType = "Pants",
                Gender = "Female",
                Height = 165,
                Weight = 55
            }, _userId);

            var ex = await act.Should().ThrowAsync<AppException>();
            ex.Which.StatusCode.Should().Be(404);
        }

        [Fact(DisplayName = "SizeRecommend: thành công + có CustomerId → lưu lại CustomerMeasurements")]
        public async Task RecommendSizeAsync_Success_SavesCustomerMeasurement()
        {
            var customerId = Guid.NewGuid();
            _db.Customers.Add(new Customer { Id = customerId, FullName = "Khách test" });
            await _db.SaveChangesAsync();

            _aiClientMock
                .Setup(c => c.RecommendSizeAsync(It.IsAny<AISizeRecommendProxyRequest>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SizeRecommendResponseDto { RecommendedSize = "M", Confidence = 0.92 });

            var result = await _service.RecommendSizeAsync(new SizeRecommendRequestDto
            {
                ProductType = "Shirt",
                Gender = "Male",
                Height = 165,
                Weight = 60,
                CustomerId = customerId
            }, _userId);

            result.RecommendedSize.Should().Be("M");
            var measurement = await _db.CustomerMeasurements.FirstOrDefaultAsync(m => m.CustomerId == customerId);
            measurement.Should().NotBeNull();
            measurement!.Height.Should().Be(165);
        }

        [Fact(DisplayName = "Forecast: chưa đủ 14 ngày lịch sử EXPORT → trả về sớm, không gọi AI client")]
        public async Task ForecastAsync_NotEnoughHistory_ReturnsEarlyWithoutCallingAIClient()
        {
            var result = await _service.ForecastAsync(new InventoryForecastRequestDto
            {
                VariantId = _variantId,
                HorizonDays = 30
            }, _userId);

            result.CurrentStock.Should().Be(20);
            result.WillRunOutInDays.Should().BeNull();
            result.Note.Should().NotBeNullOrEmpty();

            _aiClientMock.Verify(
                c => c.ForecastAsync(It.IsAny<AIForecastProxyRequest>(), It.IsAny<CancellationToken>()),
                Times.Never);
        }

        [Fact(DisplayName = "Forecast: variant không tồn tại tồn kho → ném AppException 404")]
        public async Task ForecastAsync_VariantNotFound_Throws404()
        {
            var act = async () => await _service.ForecastAsync(new InventoryForecastRequestDto
            {
                VariantId = Guid.NewGuid(),
                HorizonDays = 30
            }, _userId);

            var ex = await act.Should().ThrowAsync<AppException>();
            ex.Which.StatusCode.Should().Be(404);
        }

        [Fact(DisplayName = "Forecast: đủ dữ liệu lịch sử → gọi AI client và trả kết quả dự báo")]
        public async Task ForecastAsync_EnoughHistory_CallsAIClient()
        {
            for (int i = 0; i < 30; i++)
            {
                _db.InventoryTransactions.Add(new InventoryTransaction
                {
                    Id = Guid.NewGuid(),
                    VariantId = _variantId,
                    Type = InventoryTransactionType.EXPORT,
                    Quantity = 2,
                    QuantityBefore = 20,
                    QuantityAfter = 18,
                    CreatedAt = DateTime.UtcNow.AddDays(-i)
                });
            }
            await _db.SaveChangesAsync();

            _aiClientMock
    .Setup(c => c.ForecastAsync(It.IsAny<AIForecastProxyRequest>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new InventoryForecastResponseDto
    {
        VariantId = _variantId,
        Forecast = Enumerable.Range(1, 30)
            .Select(i => new InventoryForecastPointDto
            {
                Date = DateTime.UtcNow.Date.AddDays(i),
                PredictedQuantitySold = 4
            })
            .ToList()
    });

            var result = await _service.ForecastAsync(new InventoryForecastRequestDto
            {
                VariantId = _variantId,
                HorizonDays = 30
            }, _userId);

            result.WillRunOutInDays.Should().Be(5);
            result.NeedReorder.Should().BeTrue();
            result.CurrentStock.Should().Be(20);

            _aiClientMock.Verify(
                c => c.ForecastAsync(It.IsAny<AIForecastProxyRequest>(), It.IsAny<CancellationToken>()),
                Times.Once);
        }

        public void Dispose() => _db.Dispose();
    }
}