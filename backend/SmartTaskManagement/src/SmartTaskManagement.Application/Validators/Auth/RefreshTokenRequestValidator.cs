using FluentValidation;
using SmartTaskManagement.Application.DTOs.Auth;

namespace SmartTaskManagement.Application.Validators.Auth;

public sealed class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequestDto>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.AccessToken)
            .NotEmpty()
            .WithMessage("Access token is required")
            .MinimumLength(10)
            .WithMessage("Access token appears to be invalid");

        RuleFor(x => x.RefreshToken)
            .NotEmpty()
            .WithMessage("Refresh token is required")
            .MinimumLength(10)
            .WithMessage("Refresh token appears to be invalid");
    }
}
