using FluentValidation;

namespace Application.Features.Brands.Commands.Update;

public class UpdateBrandCommandValidation : AbstractValidator<UpdateBrandCommand>
{
    public UpdateBrandCommandValidation()
    {
        RuleFor(p => p.Name).NotEmpty();
        RuleFor(p => p.Id).NotEmpty();
    }
}