using FluentValidation;

namespace Application.Features.ProductFeatures.Commands.Update;

public class UpdateProductFeatureCommandValidation : AbstractValidator<UpdateProductFeatureCommand>
{
    public UpdateProductFeatureCommandValidation()
    {
        RuleFor(p => p.Name).NotEmpty();
        RuleFor(p => p.Id).NotEmpty();
    }
}