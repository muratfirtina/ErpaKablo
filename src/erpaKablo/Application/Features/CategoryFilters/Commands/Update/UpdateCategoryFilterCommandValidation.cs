using FluentValidation;

namespace Application.Features.CategoryFilters.Commands.Update;

public class UpdateCategoryFilterCommandValidation : AbstractValidator<UpdateCategoryFilterCommand>
{
    public UpdateCategoryFilterCommandValidation()
    {
        RuleFor(p => p.Name).NotEmpty();
        RuleFor(p => p.Id).NotEmpty();
    }
}