using FluentValidation;
using SmartTaskManagement.Application.DTOs.Auth;

namespace SmartTaskManagement.Application.Validators.Auth;

public sealed class LoginValidator : AbstractValidator<LoginDto>
{
    public LoginValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("A valid email address is required.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.");

        RuleFor(x => x.Password)
            .NotEmpty()
            .WithMessage("Password is required.")
            .MinimumLength(60)
            .WithMessage("Password must be at least 60 characters.");
    }
}
