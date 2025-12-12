using FluentValidation;
using ProjectManager.API.DTOs;

namespace ProjectManager.API.Validators;

public class CreateYumusDtoValidator : AbstractValidator<CreateYumusDto>
{
    public CreateYumusDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required")
            .MaximumLength(200).WithMessage("Task title must be less than 200 characters");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("Project ID is required");
    }
}

public class UpdateYumusDtoValidator : AbstractValidator<UpdateYumusDto>
{
    public UpdateYumusDtoValidator()
    {
        RuleFor(x => x.Title)
            .MaximumLength(200).WithMessage("Task title must be less than 200 characters");
    }
}

public class UpdateYumusStatusDtoValidator : AbstractValidator<UpdateYumusStatusDto>
{
    public UpdateYumusStatusDtoValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required")
            .Must(status => status == "Todo" || status == "InProgress" || status == "Review" || status == "Done")
            .WithMessage("Status must be Todo, InProgress, Review, or Done");
    }
}

public class CreateSubTaskDtoValidator : AbstractValidator<CreateSubTaskDto>
{
    public CreateSubTaskDtoValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Subtask title is required")
            .MaximumLength(200).WithMessage("Subtask title must be less than 200 characters");
    }
}

public class CreateTaskCommentDtoValidator : AbstractValidator<CreateTaskCommentDto>
{
    public CreateTaskCommentDtoValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Comment text is required");
    }
}