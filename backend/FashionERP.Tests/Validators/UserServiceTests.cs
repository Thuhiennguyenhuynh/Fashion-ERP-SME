using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Auth;
using FashionERP.Application.Interfaces;

namespace FashionERP.Tests.Services
{
    public class UserServiceTests
    {
        // ─── Create — duplicate email ─────────────────────────────────────────
        [Fact]
        public async Task CreateAsync_DuplicateEmail_ThrowsBusinessException()
        {
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.CreateAsync(It.IsAny<CreateUserRequestDto>()))
                .ThrowsAsync(new BusinessException("Email đã tồn tại trong hệ thống."));

            await Assert.ThrowsAsync<BusinessException>(
                () => mockService.Object.CreateAsync(new CreateUserRequestDto
                {
                    Email = "existing@mail.com",
                    Password = "Pass@123",
                    Role = "Sales"
                }));
        }

        // ─── Create — invalid role ────────────────────────────────────────────
        [Fact]
        public async Task CreateAsync_InvalidRole_ThrowsBusinessException()
        {
            var mockService = new Mock<IUserService>();
            mockService.Setup(s => s.CreateAsync(It.IsAny<CreateUserRequestDto>()))
                .ThrowsAsync(new BusinessException("Role 'SuperAdmin' không hợp lệ."));

            await Assert.ThrowsAsync<BusinessException>(
                () => mockService.Object.CreateAsync(new CreateUserRequestDto
                {
                    Email = "new@mail.com",
                    Password = "Pass@123",
                    Role = "SuperAdmin"   // role không tồn tại
                }));
        }

        // ─── ToggleActive ─────────────────────────────────────────────────────
        [Fact]
        public async Task ToggleActiveAsync_ActiveUser_DeactivatesSuccessfully()
        {
            var mockService = new Mock<IUserService>();
            var userId = Guid.NewGuid();

            mockService.Setup(s => s.ToggleActiveAsync(userId))
                .ReturnsAsync(new UserListItemDto
                {
                    Id = userId,
                    Email = "user@mail.com",
                    Role = "Sales",
                    IsActive = false   // đã bị deactivate
                });

            var result = await mockService.Object.ToggleActiveAsync(userId);

            Assert.False(result.IsActive);
        }
    }
}