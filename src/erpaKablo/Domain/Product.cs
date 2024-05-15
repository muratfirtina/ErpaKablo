using Core.Persistence.Repositories;

namespace Domain;

public class Product : Entity<string>
{
    public string Name { get; set; }
    public int? Stock { get; set; }
    public float? Price { get; set; }
    public string? Description { get; set; }
    public string? CategoryId { get; set; }
    public Category? Category { get; set; }
    public string? BrandId { get; set; }
    public Brand? Brand { get; set; }
    public ICollection<ProductImageFile>? ProductImageFiles { get; set; }
    public virtual ICollection<ProductFeature>? ProductFeatures { get; set; }
    
    public Product(string name) : base(name)
    {
        Name = name;
    }
}

