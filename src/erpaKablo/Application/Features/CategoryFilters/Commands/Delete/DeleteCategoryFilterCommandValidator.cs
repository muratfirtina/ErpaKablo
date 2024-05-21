using FluentValidation;

namespace Application.Features.CategoryFilters.Commands.Delete;

public class DeleteCategoryFilterCommandValidator : AbstractValidator<DeleteCategoryFilterCommand>
{
    public DeleteCategoryFilterCommandValidator()
    {
        RuleFor(p => p.Id).NotEmpty();
    }
}