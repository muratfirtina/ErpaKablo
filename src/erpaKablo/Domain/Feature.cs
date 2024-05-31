using System.Collections;
using Core.Persistence.Repositories;

namespace Domain;

public class Feature : Entity<string>
{
    public string? Name { get; set; }
    public ICollection<CategoryFeature> CategoryFeatures { get; set; }
    public ICollection<FeatureValue> FeatureValues { get; set; }
    //public ICollection<VariantFeatureValue> VariantFeatureValues { get; set; }

    public Feature(string? name) : base(name)
    {
        Name = name;
        CategoryFeatures = new List<CategoryFeature>();
        FeatureValues = new List<FeatureValue>();
    }
}