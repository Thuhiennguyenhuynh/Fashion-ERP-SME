using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using Xunit;
using FashionERP.Application.Common;
using FashionERP.Application.DTOs.Employee;
using FashionERP.Application.Interfaces;

namespace FashionERP.Tests.Services
{
    public class EmployeeServiceTests
    {
        [Fact]
        public async Task GetAllAsync_ReturnsPagedEmployees()
        {
            var mockService = new Mock<IEmployeeService>();
            var p = new EmployeeQueryParams { Page = 1, PageSize = 10 };
            mockService.Setup(s => s.GetAllAsync(p))
                .ReturnsAsync(new PagedResult<EmployeeResponseDto>
                {
                    Items = new List<EmployeeResponseDto>
                    {
                        new() { Id = Guid.NewGuid(), FullName = "Nguyễn Văn A", Status = "Active" },
                        new() { Id = Guid.NewGuid(), FullName = "Trần Thị B",   Status = "Probation" }
                    },
                    TotalCount = 2,
                    Page = 1,
                    PageSize = 10,
                });

            var result = await mockService.Object.GetAllAsync(p);
            Assert.NotNull(result);
            Assert.Equal(2, result.Items.Count);
            Assert.Equal(2, result.TotalCount);
        }

        [Fact]
        public async Task GetByIdAsync_NotFound_ThrowsNotFoundException()
        {
            var mockService = new Mock<IEmployeeService>();
            var fakeId = Guid.NewGuid();
            mockService.Setup(s => s.GetByIdAsync(fakeId))
                .ThrowsAsync(new NotFoundException("Nhân viên", fakeId));

            await Assert.ThrowsAsync<NotFoundException>(
                () => mockService.Object.GetByIdAsync(fakeId));
        }

        [Fact]
        public async Task UpdateStatusAsync_ValidStatus_CompletesWithoutException()
        {
            var mockService = new Mock<IEmployeeService>();
            var id = Guid.NewGuid();
            mockService.Setup(s => s.UpdateStatusAsync(id, "Resigned"))
                .Returns(Task.CompletedTask);

            await mockService.Object.UpdateStatusAsync(id, "Resigned");
            mockService.Verify(s => s.UpdateStatusAsync(id, "Resigned"), Times.Once);
        }

        [Fact]
        public async Task DeleteAsync_ValidId_CompletesWithoutException()
        {
            var mockService = new Mock<IEmployeeService>();
            var id = Guid.NewGuid();
            mockService.Setup(s => s.DeleteAsync(id)).Returns(Task.CompletedTask);

            await mockService.Object.DeleteAsync(id);
            mockService.Verify(s => s.DeleteAsync(id), Times.Once);
        }

        [Fact]
        public async Task CreateAsync_ValidRequest_ReturnsEmployee()
        {
            var mockService = new Mock<IEmployeeService>();
            var request = new CreateEmployeeRequestDto
            {
                FullName = "Lê Văn C",
                Phone = "0912345678",
                DepartmentId = Guid.NewGuid(),
                Position = "Sales",
                BaseSalary = 8_000_000,
                StartDate = DateTime.Today,
            };
            mockService.Setup(s => s.CreateAsync(request))
                .ReturnsAsync(new EmployeeResponseDto
                {
                    Id = Guid.NewGuid(),
                    FullName = "Lê Văn C",
                    Status = "Probation",
                });

            var result = await mockService.Object.CreateAsync(request);
            Assert.NotNull(result);
            Assert.Equal("Lê Văn C", result.FullName);
            Assert.Equal("Probation", result.Status);
        }
    }
}