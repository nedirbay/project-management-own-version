using FluentValidation;
using ProjectManager.API.DTOs;

namespace ProjectManager.API.Validators;

public class CreateWorkspaceDtoValidator : AbstractValidator<CreateWorkspaceDto>
{
    public CreateWorkspaceDtoValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Workspace name is required")
            .MaximumLength(100).WithMessage("Workspace name must be less than 100 characters");

        RuleFor(x => x.Color)
            .NotEmpty().WithMessage("Color is required");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be less than 500 characters");
    }
}

public class UpdateWorkspaceDtoValidator : AbstractValidator<UpdateWorkspaceDto>
{
    public UpdateWorkspaceDtoValidator()
    {
        RuleFor(x => x.Name)
            .MaximumLength(100).WithMessage("Workspace name must be less than 100 characters");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description must be less than 500 characters");
    }
}

public class AddWorkspaceMemberDtoValidator : AbstractValidator<AddWorkspaceMemberDto>
{
    public AddWorkspaceMemberDtoValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User ID is required");
    }
}