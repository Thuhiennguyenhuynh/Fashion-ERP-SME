namespace FashionERP.Application.Validators.Employee
{
    using FluentValidation;
    using FashionERP.Application.DTOs.Employee;
    using FashionERP.Domain.Common;

    public class CreateEmployeeValidator : AbstractValidator<CreateEmployeeRequestDto>
    {
        private static readonly string[] ValidGenders = ["Male", "Female", "Other"];

        public CreateEmployeeValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ và tên không được để trống")
                .Length(2, 150).WithMessage("Họ và tên phải có độ dài từ 2 đến 150 ký tự");

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
                .LessThan(System.DateTime.Today.AddYears(-15))
                    .WithMessage("Nhân viên phải ít nhất 15 tuổi")
                .When(x => x.DateOfBirth.HasValue);

            RuleFor(x => x.DepartmentId)
                .NotEmpty().WithMessage("Phòng ban không được để trống");

            RuleFor(x => x.Position)
                .NotEmpty().WithMessage("Chức vụ không được để trống")
                .MaximumLength(100).WithMessage("Chức vụ không được vượt quá 100 ký tự");

            RuleFor(x => x.BaseSalary)
                .GreaterThanOrEqualTo(0).WithMessage("Lương cơ bản phải >= 0");

            RuleFor(x => x.WorkingDaysPerMonth)
                .InclusiveBetween(1, 31)
                    .WithMessage("Số công chuẩn/tháng phải từ 1 đến 31");

            RuleFor(x => x.StartDate)
                .NotEmpty().WithMessage("Ngày vào làm không được để trống")
                .LessThanOrEqualTo(System.DateTime.Today)
                    .WithMessage("Ngày vào làm không được ở tương lai");
        }
    }

    public class UpdateEmployeeValidator : AbstractValidator<UpdateEmployeeRequestDto>
    {
        private static readonly string[] ValidGenders = ["Male", "Female", "Other"];
        private static readonly string[] ValidStatuses = ["Active", "Probation", "Resigned"];

        public UpdateEmployeeValidator()
        {
            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Họ và tên không được để trống")
                .Length(2, 150).WithMessage("Họ và tên phải có độ dài từ 2 đến 150 ký tự");

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

            RuleFor(x => x.DepartmentId)
                .NotEmpty().WithMessage("Phòng ban không được để trống");

            RuleFor(x => x.Position)
                .NotEmpty().WithMessage("Chức vụ không được để trống")
                .MaximumLength(100).WithMessage("Chức vụ không được vượt quá 100 ký tự");

            RuleFor(x => x.BaseSalary)
                .GreaterThanOrEqualTo(0).WithMessage("Lương cơ bản phải >= 0");

            RuleFor(x => x.WorkingDaysPerMonth)
                .InclusiveBetween(1, 31)
                    .WithMessage("Số công chuẩn/tháng phải từ 1 đến 31");

            RuleFor(x => x.Status)
                .NotEmpty().WithMessage("Trạng thái không được để trống")
                .Must(s => System.Array.Exists(ValidStatuses, v => v == s))
                    .WithMessage("Trạng thái phải là Active, Probation hoặc Resigned");
        }
    }
}


