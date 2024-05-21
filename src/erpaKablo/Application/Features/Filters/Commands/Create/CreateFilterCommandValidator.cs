using FluentValidation;

namespace Application.Features.Filters.Commands.Create;

public class CreateFilterCommandValidator : AbstractValidator<CreateFilterCommand>
{
    public CreateFilterCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty();
    }

}