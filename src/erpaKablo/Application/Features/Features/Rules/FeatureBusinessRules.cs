using Application.Features.Features.Consts;
using Application.Repositories;
using Core.Application.Rules;
using Core.CrossCuttingConcerns.Exceptions;
using Domain;

namespace Application.Features.Features.Rules;

public class FeatureBusinessRules : BaseBusinessRules
{
    private readonly IFeatureRepository _featureRepository;

    public FeatureBusinessRules(IFeatureRepository featureRepository)
    {
        _featureRepository = featureRepository;
    }

    public Task FeatureShouldExistWhenSelected(Feature? feature)
    {
        if (feature == null)
            throw new BusinessException(FeaturesBusinessMessages.FeatureNotExists);
        return Task.CompletedTask;
    }

    public async Task FeatureIdShouldExistWhenSelected(string id, CancellationToken cancellationToken)
    {
        Feature? feature = await _featureRepository.GetAsync(
            predicate: e => e.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        await FeatureShouldExistWhenSelected(feature);
    }
    
}