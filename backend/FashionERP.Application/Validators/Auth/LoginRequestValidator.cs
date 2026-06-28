using FluentValidation;
using FashionERP.Application.DTOs.Auth;
using FashionERP.Domain.Common;

namespace FashionERP.Application.Validators.Auth
{
    public class LoginRequestValidator : AbstractValidator<LoginRequestDto>
    {
        public LoginRequestValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email không được để trống")
                .MaximumLength(255).WithMessage("Email không được vượt quá 255 ký tự")
                .Matches(ValidationConstants.EmailPattern)
                    .WithMessage("Email không đúng định dạng (ví dụ: ten@example.com)");

            RuleFor(x => x.Password)
                .NotEmpty().WithMessage("Mật khẩu không được để trống")
                .MinimumLength(8).WithMessage("Mật khẩu phải có ít nhất 8 ký tự");
        }
    }
}

