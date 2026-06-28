namespace FashionERP.Application.Validators.Promotion
{
    using FluentValidation;
    using FashionERP.Application.DTOs.Promotion;
    using FashionERP.Domain.Common;

    public class CreatePromotionValidator : AbstractValidator<CreatePromotionRequestDto>
    {
        private static readonly string[] ValidTypes = ["Percent", "FixedAmount"];

        public CreatePromotionValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Mã khuyến mãi không được để trống")
                .Matches(ValidationConstants.PromotionCodePattern)
                    .WithMessage("Mã khuyến mãi chỉ được chứa chữ hoa và số, từ 3 đến 50 ký tự, không khoảng trắng");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Tên chương trình không được để trống")
                .Length(3, 200).WithMessage("Tên chương trình phải có độ dài từ 3 đến 200 ký tự");

            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Loại giảm giá không được để trống")
                .Must(t => System.Array.Exists(ValidTypes, v => v == t))
                    .WithMessage("Loại giảm giá phải là Percent hoặc FixedAmount");

            RuleFor(x => x.DiscountValue)
                .GreaterThan(0).WithMessage("Giá trị giảm phải lớn hơn 0");

            // Nếu là giảm theo %, giá trị phải <= 100
            RuleFor(x => x.DiscountValue)
                .LessThanOrEqualTo(100)
                    .WithMessage("Phần trăm giảm giá không được vượt quá 100%")
                .When(x => x.Type == "Percent");

            RuleFor(x => x.MinOrderValue)
                .GreaterThanOrEqualTo(0).WithMessage("Đơn tối thiểu phải >= 0");

            RuleFor(x => x.UsageLimit)
                .GreaterThan(0).WithMessage("Giới hạn lượt dùng phải lớn hơn 0")
                .When(x => x.UsageLimit.HasValue);

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Ngày bắt đầu không được để trống");

            RuleFor(x => x.EndDate)
                .NotEmpty().WithMessage("Ngày kết thúc không được để trống")
                .GreaterThan(x => x.StartDate)
                    .WithMessage("Ngày kết thúc phải sau ngày bắt đầu");
        }
    }
}


