using Core.Persistence.Repositories;

namespace Domain;

public class Product : Entity<int>
{
    public string? Name { get; set; }
    public int Stock { get; set; }
    public float Price { get; set; }
    public int CategoryId { get; set; }
    public Category? Category { get; set; }
    public int BrandId { get; set; }
    public Brand? Brand { get; set; }
    public ICollection<ProductImageFile>? ProductImageFiles { get; set; }
    public virtual ICollection<ProductFeature>? ProductFeatures { get; set; }
}