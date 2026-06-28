namespace FashionERP.Application.Validators.Auth
{
    using FluentValidation;
    using FashionERP.Application.DTOs.Auth;
    using FashionERP.Domain.Common;

    public class ChangePasswordValidator : AbstractValidator<ChangePasswordRequestDto>
    {
        public ChangePasswordValidator()
        {
            RuleFor(x => x.CurrentPassword)
                .NotEmpty().WithMessage("Mật khẩu hiện tại không được để trống");

            RuleFor(x => x.NewPassword)
                .NotEmpty().WithMessage("Mật khẩu mới không được để trống")
                .Matches(ValidationConstants.PasswordPattern)
                    .WithMessage("Mật khẩu mới phải có ít nhất 8 ký tự, bao gồm chữ hoa, chữ thường và số");

            RuleFor(x => x.ConfirmNewPassword)
                .NotEmpty().WithMessage("Xác nhận mật khẩu không được để trống")
                .Equal(x => x.NewPassword).WithMessage("Xác nhận mật khẩu không khớp với mật khẩu mới");

            RuleFor(x => x.NewPassword)
                .NotEqual(x => x.CurrentPassword)
                    .WithMessage("Mật khẩu mới không được trùng với mật khẩu hiện tại");
        }
    }
}
