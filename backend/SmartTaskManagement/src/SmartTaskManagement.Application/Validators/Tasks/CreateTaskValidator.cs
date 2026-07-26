using FluentValidation;
using SmartTaskManagement.Application.DTOs.Tasks;

namespace SmartTaskManagement.Application.Validators.Tasks;

public sealed class CreateTaskValidator : AbstractValidator<CreateTaskDto>
{
    public CreateTaskValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .NotEmpty().WithMessage("Description is required.")
            .MaximumLength(2000).WithMessage("Description must not exceed 2000 characters.");

        RuleFor(x => x.Priority)
            .InclusiveBetween(1, 4).WithMessage("Priority must be 1=Low, 2=Medium, 3=High, 4=Critical.");

        // Allow any date - users may want to create tasks with past or present due dates
        // The frontend will show overdue status for past dates
    }
}
