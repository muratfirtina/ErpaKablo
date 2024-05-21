using Core.Persistence.Repositories;

namespace Domain;

public class ProductFeature : Entity<string>
{
    public string? Name { get; set; } // Örneğin: "Kablo Yapısı"
    
    public virtual ICollection<Product> Products { get; set; }
    public virtual ICollection<Feature> Features { get; set; }
    
    public ProductFeature(string? name) : base(name)
    {
        Name = name;
    }
    public ProductFeature() { }
}
