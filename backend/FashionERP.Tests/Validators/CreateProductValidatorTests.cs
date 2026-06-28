using FluentValidation.TestHelper;
using FashionERP.Application.DTOs.Product;
using FashionERP.Application.Validators.Product;
using System;
using Xunit;

namespace FashionERP.Tests.Validators
{
    public class CreateProductValidatorTests
    {
        private readonly CreateProductValidator _validator = new();

        // ─── Name ────────────────────────────────────────────────────────────
        [Fact]
        public void Name_Empty_ShouldHaveValidationError()
        {
            var dto = ValidDto();
            dto.Name = string.Empty;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Name)
                  .WithErrorMessage("Tên sản phẩm không được để trống");
        }

        // ─── BasePrice ───────────────────────────────────────────────────────
        [Fact]
        public void BasePrice_Zero_ShouldHaveValidationError()
        {
            var dto = ValidDto();
            dto.BasePrice = 0;

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.BasePrice)
                  .WithErrorMessage("Giá bán cơ bản phải lớn hơn 0");
        }

        // ─── Gender ──────────────────────────────────────────────────────────
        [Fact]
        public void Gender_InvalidValue_ShouldHaveValidationError()
        {
            var dto = ValidDto();
            dto.Gender = "InvalidGender";

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.Gender);
        }

        // ─── Happy path ──────────────────────────────────────────────────────
        [Fact]
        public void ValidDto_ShouldNotHaveAnyValidationError()
        {
            var dto = ValidDto();

            var result = _validator.TestValidate(dto);

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ─── Helper ──────────────────────────────────────────────────────────
        private static CreateProductRequestDto ValidDto() => new()
        {
            Name = "Áo thun basic",
            CategoryId = Guid.NewGuid(),
            Gender = "Unisex",
            BasePrice = 250_000,
            Status = "Draft"
        };
    }
}