using Core.Persistence.Repositories;

namespace Domain;

public class ProductFeature : Entity<int>
{
    public string FeatureGroupName { get; set; } // Örneğin: "Kablo Yapısı"
    public int ProductId { get; set; }
    public virtual Product Product { get; set; }
    public virtual ICollection<Feature> Features { get; set; }
}
