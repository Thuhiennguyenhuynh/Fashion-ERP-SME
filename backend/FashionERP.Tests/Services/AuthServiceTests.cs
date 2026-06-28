using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FashionERP.Application.Interfaces;
using FashionERP.Application.DTOs.Auth;
using FashionERP.Application.Common;

namespace FashionERP.Tests.Services
{
    public class AuthServiceTests
    {
        [Fact]
        public async Task LoginAsync_WrongPassword_ThrowsException()
        {
            var mockService = new Mock<IAuthService>();
            mockService.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDto>()))
                .ThrowsAsync(new BusinessException("Sai mật khẩu"));

            await Assert.ThrowsAsync<BusinessException>(
                () => mockService.Object.LoginAsync(new LoginRequestDto { Email = "test@test.com", Password = "wrong" }));
        }

        [Fact]
        public async Task LoginAsync_ValidCredentials_ReturnsToken()
        {
            var mockService = new Mock<IAuthService>();
            mockService.Setup(s => s.LoginAsync(It.IsAny<LoginRequestDto>()))
                // Sửa thành AuthResponseDto
                .ReturnsAsync(new AuthResponseDto { AccessToken = "token123" });

            var result = await mockService.Object.LoginAsync(new LoginRequestDto { Email = "a@b.com", Password = "pass" });
            Assert.Equal("token123", result.AccessToken);
        }

        [Fact]
        public async Task ChangePasswordAsync_SamePassword_ThrowsException()
        {
            var mockService = new Mock<IAuthService>();
            mockService.Setup(s => s.ChangePasswordAsync(
                    It.IsAny<Guid>(), It.IsAny<ChangePasswordRequestDto>()))
                .ThrowsAsync(new BusinessException("Mật khẩu mới không được trùng cũ"));

            await Assert.ThrowsAsync<BusinessException>(
                () => mockService.Object.ChangePasswordAsync(
                    Guid.NewGuid(),
                    new ChangePasswordRequestDto { CurrentPassword = "same", NewPassword = "same" }));
        }
    }
}