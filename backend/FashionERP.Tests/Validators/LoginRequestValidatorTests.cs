using FluentValidation.TestHelper;
using FashionERP.Application.DTOs.Auth;
using FashionERP.Application.Validators.Auth;
using Xunit;

namespace FashionERP.Tests.Validators
{
    public class LoginRequestValidatorTests
    {
        private readonly LoginRequestValidator _validator = new();

        // ─── Email rỗng ───────────────────────────────────────────────────────
        [Fact]
        public void Email_Empty_ShouldHaveValidationError()
        {
            var dto = new LoginRequestDto(string.Empty, "Password@1");

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Email);
        }

        // ─── Password rỗng ───────────────────────────────────────────────────
        [Fact]
        public void Password_Empty_ShouldHaveValidationError()
        {
            var dto = new LoginRequestDto("test@test.com", string.Empty);

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Password);
        }
    }
}