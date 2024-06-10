namespace Application.Features.Products.Dtos;

public class ProductVariantDto
{
    //public string Id { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public List<VariantFeatureDto> Features { get; set; }
}