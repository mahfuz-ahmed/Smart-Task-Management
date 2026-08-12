using FluentValidation;
using SmartTaskManagement.Application.DTOs.Tasks;
public sealed class AssignTaskValidator: AbstractValidator<AssignTaskDto>
{
    public AssignTaskValidator()
    {
        RuleFor(x => x.AssignedToUserId)
            .NotEmpty()
            .WithMessage("Assigned user is required.");
    }
}