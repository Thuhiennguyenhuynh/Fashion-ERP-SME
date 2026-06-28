using FluentValidation.TestHelper;
using FashionERP.Application.DTOs.Auth;
using FashionERP.Application.Validators.Auth;
using Xunit;

namespace FashionERP.Tests.Validators
{
    public class LoginRequestValidatorTests
    {
        private readonly LoginRequestValidator _validator = new();

        [Fact]
        public void Email_Empty_ShouldHaveValidationError()
        {
            var dto = new LoginRequestDto { Email = string.Empty, Password = "Password@1" };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        [Fact]
        public void Password_Empty_ShouldHaveValidationError()
        {
            var dto = new LoginRequestDto { Email = "test@test.com", Password = string.Empty };
            var result = _validator.TestValidate(dto);
            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
    }
}