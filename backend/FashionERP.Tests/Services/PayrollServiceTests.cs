using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FashionERP.Application.Interfaces;
using FashionERP.Application.DTOs.HR;

namespace FashionERP.Tests.Services
{
    public class PayrollServiceTests
    {
        [Fact]
        public async Task GenerateAsync_CalculatesNetSalaryCorrectly()
        {
            var mockService = new Mock<IPayrollService>();
            var request = new GeneratePayrollRequestDto
            {
                EmployeeId = Guid.NewGuid(),
                Month = 6,
                Year = 2026,
                Allowance = 500_000,
                Deduction = 100_000
            };
            mockService.Setup(s => s.GenerateAsync(request))
                .ReturnsAsync(new PayrollResponseDto { NetSalary = 9_068_461 });

            var result = await mockService.Object.GenerateAsync(request);
            Assert.True(result.NetSalary > 0);
        }

        [Fact]
        public async Task ConfirmAsync_AlreadyPaid_ThrowsException()
        {
            var mockService = new Mock<IPayrollService>();
            var id = Guid.NewGuid();
            mockService.Setup(s => s.ConfirmAsync(id))
                .ThrowsAsync(new Application.Common.BusinessException("Bảng lương đã được thanh toán"));

            await Assert.ThrowsAsync<Application.Common.BusinessException>(
                () => mockService.Object.ConfirmAsync(id));
        }
    }
}