using FluentValidation;

namespace Application.Features.FeatureValues.Commands.Update;

public class UpdateFeatureValueCommandValidation : AbstractValidator<UpdateFeatureValueCommand>
{
    public UpdateFeatureValueCommandValidation()
    {
        RuleFor(p => p.Value).NotEmpty();
        RuleFor(p => p.Id).NotEmpty();
    }
}