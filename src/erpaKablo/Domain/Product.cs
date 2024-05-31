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
    public string Sku { get; set; }
    
    public VariantGroup? VariantGroup { get; set; }
    public string? VariantGroupId { get; set; }
    public virtual ICollection<ProductImageFile>? ProductImageFiles { get; set; }
    public virtual ICollection<ProductVariant>? ProductVariants { get; set; }
    

    
    public Product(string? name, string? sku) : base(name,sku)
    {
        Name = name;
        Sku = sku;
        ProductVariants = new List<ProductVariant>();
    }
}

