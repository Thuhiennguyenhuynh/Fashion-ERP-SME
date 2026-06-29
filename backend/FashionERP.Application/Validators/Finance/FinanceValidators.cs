namespace FashionERP.Application.Validators.Finance
{
    using FluentValidation;
    using FashionERP.Application.DTOs.Finance;

    public class CreateCashTransactionValidator : AbstractValidator<CreateCashTransactionRequestDto>
    {
        private static readonly string[] ValidTypes = ["INCOME", "EXPENSE"];

        public CreateCashTransactionValidator()
        {
            RuleFor(x => x.Type)
                .NotEmpty().WithMessage("Loại giao dịch không được để trống")
                .Must(t => System.Array.Exists(ValidTypes, v => v == t.ToUpper()))
                    .WithMessage("Loại giao dịch phải là INCOME hoặc EXPENSE");

            RuleFor(x => x.Category)
                .NotEmpty().WithMessage("Danh mục không được để trống")
                .MaximumLength(100).WithMessage("Danh mục tối đa 100 ký tự");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Số tiền phải lớn hơn 0");
        }
    }
}
