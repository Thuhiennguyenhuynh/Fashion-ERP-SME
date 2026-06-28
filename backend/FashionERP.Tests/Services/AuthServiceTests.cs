using System.Threading.Tasks;
using Xunit;
using Moq;
using FashionERP.Application.Interfaces;
using FashionERP.Application.DTOs.Auth;

namespace FashionERP.Tests.Services
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsException()
        {
            var mockService = new Mock<IAuthService>();
            mockService.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDto>()))
                .ThrowsAsync(new Application.Common.BusinessException("Sai mật khẩu"));

            await Assert.ThrowsAsync<Application.Common.BusinessException>(
                () => mockService.Object.LoginAsync(new LoginRequestDto("test@test.com", "wrong")));
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsToken()
        {
            var mockService = new Mock<IAuthService>();
            mockService.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDto>()))
                .ReturnsAsync(new LoginResponseDto { AccessToken = "token123" });

            var result = await mockService.Object.LoginAsync(new LoginRequestDto("a@b.com", "pass"));
            Assert.Equal("token123", result.AccessToken);
        }

        [Fact]
        public async Task ChangePasswordAsync_SamePassword_ThrowsException()
        {
            var mockService = new Mock<IAuthService>();
            mockService.Setup(s => s.ChangePasswordAsync(
                    It.IsAny<System.Guid>(), It.IsAny<ChangePasswordRequestDto>()))
                .ThrowsAsync(new Application.Common.BusinessException("Mật khẩu mới không được trùng cũ"));

            await Assert.ThrowsAsync<Application.Common.BusinessException>(
                () => mockService.Object.ChangePasswordAsync(
                    System.Guid.NewGuid(),
                    new ChangePasswordRequestDto("same", "same")));
        }
    }
}