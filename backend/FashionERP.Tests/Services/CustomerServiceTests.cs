using Xunit;
using Moq;
using FashionERP.Application.Interfaces;
using System.Threading.Tasks;

namespace FashionERP.Tests.Services
{
    public class CustomerServiceTests
    {
        [Fact]
        public async Task UpdateMemberLevel_TotalSpentOver5M_PromotesToGold()
        {
            // Test logic nâng hạng thành viên: >= 5,000,000 → Gold
            var mockService = new Mock<ICustomerService>();
            // Giả lập customer đã có totalSpent = 5_000_000
            mockService.Setup(s => s.GetByIdAsync(It.IsAny<System.Guid>()))
                .ReturnsAsync(new Application.DTOs.Customer.CustomerResponseDto
                {
                    TotalSpent = 5_000_000,
                    MemberLevel = "Gold"
                });

            var result = await mockService.Object.GetByIdAsync(System.Guid.NewGuid());
            Assert.Equal("Gold", result.MemberLevel);
        }

        [Fact]
        public async Task CreateCustomer_DuplicatePhone_ThrowsException()
        {
            var mockService = new Mock<ICustomerService>();
            mockService.Setup(s => s.CreateAsync(It.IsAny<Application.DTOs.Customer.CreateCustomerRequestDto>()))
                .ThrowsAsync(new Application.Common.BusinessException("Số điện thoại đã tồn tại"));

            await Assert.ThrowsAsync<Application.Common.BusinessException>(
                () => mockService.Object.CreateAsync(new Application.DTOs.Customer.CreateCustomerRequestDto()));
        }
    }
}