using FluentValidation.TestHelper;
using FashionERP.Application.DTOs.Promotion;
using FashionERP.Application.Validators.Promotion;
using System;
using Xunit;

namespace FashionERP.Tests.Validators
{
    public class CreatePromotionValidatorTests
    {
        private readonly CreatePromotionValidator _validator = new();

        // ─── DiscountValue > 100 khi Percent ─────────────────────────────────
        [Fact]
        public void DiscountValue_Over100_WhenPercent_ShouldHaveError()
        {
            var dto = ValidDto();
            dto.Type = "Percent";
            dto.DiscountValue = 150; // vượt 100%

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.DiscountValue)
                  .WithErrorMessage("Phần trăm giảm giá không được vượt quá 100%");
        }

        // ─── EndDate trước StartDate ──────────────────────────────────────────
        [Fact]
        public void EndDate_BeforeStartDate_ShouldHaveError()
        {
            var dto = ValidDto();
            dto.StartDate = DateTime.UtcNow.AddDays(5);
            dto.EndDate = DateTime.UtcNow.AddDays(1); // trước StartDate

            var result = _validator.TestValidate(dto);

            result.ShouldHaveValidationErrorFor(x => x.EndDate)
                  .WithErrorMessage("Ngày kết thúc phải sau ngày bắt đầu");
        }

        // ─── Happy path ──────────────────────────────────────────────────────
        [Fact]
        public void ValidDto_ShouldNotHaveAnyValidationError()
        {
            var result = _validator.TestValidate(ValidDto());

            result.ShouldNotHaveAnyValidationErrors();
        }

        // ─── Helper ──────────────────────────────────────────────────────────
        private static CreatePromotionRequestDto ValidDto() => new()
        {
            Code = "SALE20",
            Name = "Giảm 20% mùa hè",
            Type = "Percent",
            DiscountValue = 20,
            MinOrderValue = 500_000,
            StartDate = DateTime.UtcNow,
            EndDate = DateTime.UtcNow.AddDays(30),
            UsageLimit = 100
        };
    }
}