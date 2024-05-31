using Core.Persistence.Repositories;

namespace Domain;

public class ProductVariant : Entity<string>
{
    public string ProductId { get; set; }
    public Product Product { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public ICollection<VariantFeatureValue> VariantFeatureValues { get; set; }

    public ProductVariant(string productId, decimal price, int stock)
    {
        ProductId = productId;
        Price = price;
        Stock = stock;
        VariantFeatureValues = new List<VariantFeatureValue>();
    }

    public ProductVariant()
    {
        VariantFeatureValues = new List<VariantFeatureValue>();
    }
}