using Core.Persistence.Repositories;

namespace Domain;

public class VariantFeatureValue
{
    public string ProductVariantId { get; set; }
    public ProductVariant ProductVariant { get; set; }
    public string FeatureId { get; set; }
    public Feature Feature { get; set; }
    public string FeatureValueId { get; set; }
    public FeatureValue FeatureValue { get; set; }

    public VariantFeatureValue(string productVariantId, string featureId, string featureValueId)
    {
        ProductVariantId = productVariantId;
        FeatureId = featureId;
        FeatureValueId = featureValueId;
    }

    public VariantFeatureValue()
    {
        
    }
}