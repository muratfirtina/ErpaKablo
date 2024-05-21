using Application.Features.ProductFeatures.Consts;
using Application.Repositories;
using Core.Application.Rules;
using Core.CrossCuttingConcerns.Exceptions;
using Domain;

namespace Application.Features.ProductFeatures.Rules;

public class ProductFeatureBusinessRules : BaseBusinessRules
{
    private readonly IProductFeatureRepository _productFeatureRepository;

    public ProductFeatureBusinessRules(IProductFeatureRepository productFeatureRepository)
    {
        _productFeatureRepository = productFeatureRepository;
    }

    public Task ProductFeatureShouldExistWhenSelected(ProductFeature? productFeature)
    {
        if (productFeature == null)
            throw new BusinessException(ProductFeaturesBusinessMessages.ProductFeatureNotExists);
        return Task.CompletedTask;
    }

    public async Task ProductFeatureIdShouldExistWhenSelected(string id, CancellationToken cancellationToken)
    {
        ProductFeature? productFeature = await _productFeatureRepository.GetAsync(
            predicate: e => e.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        await ProductFeatureShouldExistWhenSelected(productFeature);
    }
    
}