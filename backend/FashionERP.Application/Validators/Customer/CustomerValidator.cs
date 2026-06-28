namespace FashionERP.Application.Validators.Customer
{
    using FluentValidation;
    using FashionERP.Application.DTOs.Customer;
    using FashionERP.Domain.Common;

    public class CreateCustomerValidator : AbstractValidator<CreateCustomerRequestDto>
    {
        private static readonly string[] ValidGenders = ["Male", "Female", "Other"];

        public CreateCustomerValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Tên khách hàng không được để trống")
                .Length(2, 150).WithMessage("Tên khách hàng phải có độ dài từ 2 đến 150 ký tự");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Số điện thoại không được để trống")
                .Matches(ValidationConstants.PhonePattern)
                    .WithMessage("Số điện thoại phải là số Việt Nam hợp lệ gồm 10 số, bắt đầu bằng số 0");

            RuleFor(x => x.Email)
                .Matches(ValidationConstants.EmailPattern)
                    .WithMessage("Email không đúng định dạng (ví dụ: ten@example.com)")
                .When(x => !string.IsNullOrEmpty(x.Email));

            RuleFor(x => x.Gender)
                .Must(g => g == null || System.Array.Exists(ValidGenders, v => v == g))
                    .WithMessage("Giới tính phải là Male, Female hoặc Other");

            RuleFor(x => x.DateOfBirth)
                .LessThan(System.DateTime.Today)
                    .WithMessage("Ngày sinh không hợp lệ")
                .When(x => x.DateOfBirth.HasValue);

            RuleFor(x => x.Note)
                .MaximumLength(300).WithMessage("Ghi chú không được vượt quá 300 ký tự");
        }
    }

    public class CustomerMeasurementValidator : AbstractValidator<CustomerMeasurementDto>
    {
        public CustomerMeasurementValidator()
        {
            RuleFor(x => x.Height)
                .InclusiveBetween(100, 250).WithMessage("Chiều cao phải từ 100 đến 250 cm")
                .When(x => x.Height.HasValue);

            RuleFor(x => x.Weight)
                .InclusiveBetween(30, 200).WithMessage("Cân nặng phải từ 30 đến 200 kg")
                .When(x => x.Weight.HasValue);

            RuleFor(x => x.Chest)
                .InclusiveBetween(0, 300).WithMessage("Vòng ngực phải từ 0 đến 300 cm")
                .When(x => x.Chest.HasValue);

            RuleFor(x => x.Waist)
                .InclusiveBetween(0, 300).WithMessage("Vòng eo phải từ 0 đến 300 cm")
                .When(x => x.Waist.HasValue);

            RuleFor(x => x.Hip)
                .InclusiveBetween(0, 300).WithMessage("Vòng hông phải từ 0 đến 300 cm")
                .When(x => x.Hip.HasValue);
        }
    }
}

