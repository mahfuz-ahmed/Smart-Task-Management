using FluentValidation;
using SmartTaskManagement.Application.DTOs.Projects;

namespace SmartTaskManagement.Application.Validators.ProjectMembers
{
    public class CreateProjectMembersValidator : AbstractValidator<AddProjectMemberDto>
    {
        public CreateProjectMembersValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty()
                .WithMessage("User is required.");

            RuleFor(x => x.ProjectRole)
                .IsInEnum()
                .WithMessage("Invalid project role.");
        }
    }
}