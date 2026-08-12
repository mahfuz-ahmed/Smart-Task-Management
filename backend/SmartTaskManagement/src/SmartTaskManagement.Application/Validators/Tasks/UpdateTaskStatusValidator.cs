using FluentValidation;
using SmartTaskManagement.Application.DTOs.Tasks;

namespace SmartTaskManagement.Application.Validators.Tasks
{
    public class UpdateTaskStatusValidator : AbstractValidator<UpdateTaskStatusDto>
    {
        public UpdateTaskStatusValidator()
        {
            RuleFor(x => x.Status)
                .IsInEnum()
                .WithMessage("Invalid task status.");
        }
    }
}