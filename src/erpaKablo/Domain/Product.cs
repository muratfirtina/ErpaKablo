using Core.Persistence.Repositories;

namespace Domain;

public class Product : Entity<string>
{
    public string Name { get; set; }
    public string? Description { get; set; }
    public string? CategoryId { get; set; }
    public Category? Category { get; set; }
    public string? BrandId { get; set; }
    public Brand? Brand { get; set; }
    public string VaryantGroupID { get; set; }
    public string? Sku { get; set; }
    public decimal? Price { get; set; }
    public int Stock { get; set; } = 0;
    public int? Tax { get; set; }
    
    public virtual ICollection<ProductImageFile>? ProductImageFiles { get; set; }
    public virtual ICollection<ProductFeatureValue>? ProductFeatureValues { get; set; }
    

    
    public Product(string? name, string? sku) : base(name,sku)
    {
        Name = name;
        Sku = sku;
        ProductImageFiles = new List<ProductImageFile>();
        ProductFeatureValues = new List<ProductFeatureValue>();
    }
    
    
    
}

