using Application.Features.FeatureValues.Consts;
using Application.Repositories;
using Core.Application.Rules;
using Core.CrossCuttingConcerns.Exceptions;
using Domain;

namespace Application.Features.FeatureValues.Rules;

public class FeatureValueBusinessRules : BaseBusinessRules
{
    private readonly IFeatureValueRepository _featureRepository;

    public FeatureValueBusinessRules(IFeatureValueRepository featureRepository)
    {
        _featureRepository = featureRepository;
    }

    public Task FeatureValueShouldExistWhenSelected(FeatureValue? feature)
    {
        if (feature == null)
            throw new BusinessException(FeatureValuesBusinessMessages.FeatureValueNotExists);
        return Task.CompletedTask;
    }

    public async Task FeatureValueIdShouldExistWhenSelected(string id, CancellationToken cancellationToken)
    {
        FeatureValue? feature = await _featureRepository.GetAsync(
            predicate: e => e.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        await FeatureValueShouldExistWhenSelected(feature);
    }
    
}