using FluentValidation;

namespace Application.Features.Filters.Commands.Delete;

public class DeleteFilterCommandValidator : AbstractValidator<DeleteFilterCommand>
{
    public DeleteFilterCommandValidator()
    {
        RuleFor(p => p.Id).NotEmpty();
    }
}