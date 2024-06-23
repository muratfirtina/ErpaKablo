using Core.Persistence.Repositories;

namespace Domain;

public class FeatureValue : Entity<string>
{
    public string? Name { get; set; }
    public string? FeatureId { get; set; }
    public Feature? Feature { get; set; }
    //public ICollection<VariantFeatureValue> VariantFeatureValues { get; set; }

    public FeatureValue(string name, string? featureId) : base(name)
    {
        Name = name;
        FeatureId = featureId;
    }
}