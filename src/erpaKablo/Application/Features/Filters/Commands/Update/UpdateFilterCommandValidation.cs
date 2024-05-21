using FluentValidation;

namespace Application.Features.Filters.Commands.Update;

public class UpdateFilterCommandValidation : AbstractValidator<UpdateFilterCommand>
{
    public UpdateFilterCommandValidation()
    {
        RuleFor(p => p.Name).NotEmpty();
        RuleFor(p => p.Id).NotEmpty();
    }
}