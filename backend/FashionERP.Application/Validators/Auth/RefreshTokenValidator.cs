namespace FashionERP.Application.Validators.Auth
{
    using FluentValidation;
    using FashionERP.Application.DTOs.Auth;

    public class RefreshTokenValidator : AbstractValidator<RefreshTokenRequestDto>
    {
        public RefreshTokenValidator()
        {
            RuleFor(x => x.AccessToken)
                .NotEmpty().WithMessage("Access token không được để trống");

            RuleFor(x => x.RefreshToken)
                .NotEmpty().WithMessage("Refresh token không được để trống");
        }
    }
}
