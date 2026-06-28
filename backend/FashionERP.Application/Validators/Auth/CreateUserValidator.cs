namespace FashionERP.Application.Validators.Auth
{
    using FluentValidation;
    using FashionERP.Application.DTOs.Auth;
    using FashionERP.Domain.Common;
    using FashionERP.Domain.Enums;

    public class CreateUserValidator : AbstractValidator<CreateUserRequestDto>
    {
        private static readonly string[] ValidRoles =
            ["Admin", "Manager", "Sales", "Warehouse", "Accountant"];

        public CreateUserValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống")
                .MaximumLength(255).WithMessage("Email không được vượt quá 255 ký tự")
                .Matches(ValidationConstants.EmailPattern)
                    .WithMessage("Email không đúng định dạng (ví dụ: ten@example.com)");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống")
                .Matches(ValidationConstants.PasswordPattern)
                    .WithMessage("Mật khẩu phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường và số");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Vai trò không được để trống")
                .Must(r => System.Array.Exists(ValidRoles, v => v == r))
                    .WithMessage("Vai trò phải là một trong: Admin, Manager, Sales, Warehouse, Accountant");
        }
    }
}

