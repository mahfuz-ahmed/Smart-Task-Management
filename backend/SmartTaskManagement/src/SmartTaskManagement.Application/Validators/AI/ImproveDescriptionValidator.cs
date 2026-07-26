using System.Text.RegularExpressions;
using FluentValidation;
using SmartTaskManagement.Application.DTOs.AI;

namespace SmartTaskManagement.Application.Validators.AI;

public sealed class ImproveDescriptionValidator : AbstractValidator<ImproveDescriptionDto>
{
    private static readonly string[] ProfanityWords =
    {
        "damn", "hell", "crap", "shit", "fuck", "bitch", "bastard", "asshole"
    };

    private static readonly Regex EmailRegex = new(
        @"[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex PhoneRegex = new(
        @"(?:\+\d{1,3}[\s-]?)?(?:\(?\d{3}\)?[\s-]?)?\d{3}[\s-]?\d{4}",
        RegexOptions.Compiled);

    public ImproveDescriptionValidator()
    {
        RuleFor(x => x.Description)
            .Cascade(CascadeMode.Stop)
            .NotEmpty().WithMessage("Description cannot be empty.")
            .MaximumLength(1000).WithMessage("Description must not exceed 1000 characters.")
            .Must(NotContainProfanity).WithMessage("Description contains inappropriate language.")
            .Must(NotContainPii).WithMessage("Description must not contain personal data such as email address or phone number.");

        RuleFor(x => x.TaskTitle)
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters.");
    }

    private static bool NotContainProfanity(string description)
    {
        if (string.IsNullOrWhiteSpace(description)) return true;
        var normalized = description.ToLowerInvariant();
        return !ProfanityWords.Any(word => normalized.Contains(word));
    }

    private static bool NotContainPii(string description)
    {
        if (string.IsNullOrWhiteSpace(description)) return true;
        return !EmailRegex.IsMatch(description) && !PhoneRegex.IsMatch(description);
    }
}
