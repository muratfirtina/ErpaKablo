using Core.Persistence.Repositories;

namespace Domain;

public class Feature : Entity<int>
{
    public string Name { get; set; } // Örneğin: "İletken"
    public string Value { get; set; } // Örneğin: "Bükülü Kalaylı Bakır Teller"
    public int ProductFeatureId { get; set; }
    public virtual ProductFeature ProductFeature { get; set; }
}