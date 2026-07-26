using FluentValidation;
using SmartTaskManagement.Application.DTOs.Comments;

namespace SmartTaskManagement.Application.Validators.Comments;

public sealed class CreateCommentValidator : AbstractValidator<CreateCommentDto>
{
    public CreateCommentValidator()
    {
        RuleFor(x => x.Content)
            .NotEmpty().WithMessage("Comment content is required.")
            .MaximumLength(1000).WithMessage("Comment must not exceed 1000 characters.");
    }
}
