namespace Domain;

public class ProductVariantGroup
{
    public string ProductId { get; set; }
    public Product Product { get; set; }
    public string VariantGroupId { get; set; }
    public VariantGroup VariantGroup { get; set; }
    
}