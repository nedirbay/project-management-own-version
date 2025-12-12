using FluentValidation;
using ProjectManager.API.DTOs;

namespace ProjectManager.API.Validators;

public class CreateDailyReportDtoValidator : AbstractValidator<CreateDailyReportDto>
{
    public CreateDailyReportDtoValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required");

        RuleFor(x => x.WorkspaceId)
            .NotEmpty().WithMessage("Workspace ID is required");

        RuleFor(x => x.WorkDescription)
            .NotEmpty().WithMessage("Work description is required")
            .MinimumLength(10).WithMessage("Work description must be at least 10 characters");
    }
}

public class UpdateDailyReportDtoValidator : AbstractValidator<UpdateDailyReportDto>
{
    public UpdateDailyReportDtoValidator()
    {
        RuleFor(x => x.WorkDescription)
            .MinimumLength(10).When(x => !string.IsNullOrEmpty(x.WorkDescription))
            .WithMessage("Work description must be at least 10 characters");
    }
}