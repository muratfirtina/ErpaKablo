using Core.Persistence.Repositories;

namespace Domain;

public class Feature : Entity<string>
{
    public string Name { get; set; } // Örneğin: "İletken"
    public string Value { get; set; } // Örneğin: "Bükülü Kalaylı Bakır Teller"
    public string ProductFeatureId { get; set; }
    public virtual ProductFeature ProductFeature { get; set; }
    
    public Feature(string name, string value) : base(name)
    {
        Name = name;
        Value = value;
    }
}