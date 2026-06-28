using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FashionERP.Application.Interfaces;
using FashionERP.Application.DTOs.Order;
using FashionERP.Application.Common;

namespace FashionERP.Tests.Services
{
    public class OrderServiceTests
    {
        // --- CreateAsync ---
        [Fact]
        public async Task CreateAsync_ValidRequest_ReturnsOrder()
        {
            // Arrange
            var mockService = new Mock<IOrderService>();
            var request = new CreateOrderRequestDto(/* fill fields */);
            var staffId = Guid.NewGuid();
            mockService.Setup(s => s.CreateAsync(request, staffId))
                .ReturnsAsync(new OrderResponseDto { /* ... */ });

            // Act
            var result = await mockService.Object.CreateAsync(request, staffId);

            // Assert
            Assert.NotNull(result);
        }

        [Fact]
        public async Task CancelAsync_ValidId_CompletesWithoutException()
        {
            var mockService = new Mock<IOrderService>();
            var orderId = Guid.NewGuid();
            mockService.Setup(s => s.CancelAsync(orderId)).Returns(Task.CompletedTask);

            await mockService.Object.CancelAsync(orderId); // should not throw
        }

        [Fact]
        public async Task CompleteAsync_ValidId_CompletesWithoutException()
        {
            var mockService = new Mock<IOrderService>();
            var orderId = Guid.NewGuid();
            mockService.Setup(s => s.CompleteAsync(orderId)).Returns(Task.CompletedTask);

            await mockService.Object.CompleteAsync(orderId);
        }

        [Fact]
        public async Task GetAllAsync_ReturnsPagedResult()
        {
            var mockService = new Mock<IOrderService>();
            mockService.Setup(s => s.GetAllAsync(null, null, null, null, 1, 20))
                .ReturnsAsync(new PagedResult<OrderResponseDto>(new(), 0, 1, 20));

            var result = await mockService.Object.GetAllAsync(null, null, null, null, 1, 20);
            Assert.NotNull(result);
        }
    }
}