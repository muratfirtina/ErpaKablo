using FluentValidation;

namespace Application.Features.CategoryFilters.Commands.Create;

public class CreateCategoryFilterCommandValidator : AbstractValidator<CreateCategoryFilterCommand>
{
    public CreateCategoryFilterCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty();
    }

}