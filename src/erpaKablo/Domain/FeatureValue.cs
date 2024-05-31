using Core.Persistence.Repositories;

namespace Domain;

public class FeatureValue : Entity<string>
{
    public string Value { get; set; }
    public string FeatureId { get; set; }
    public Feature Feature { get; set; }
    //public ICollection<VariantFeatureValue> VariantFeatureValues { get; set; }

    public FeatureValue(string value, string featureId) : base(value)
    {
        Value = value;
        FeatureId = featureId;
    }
}