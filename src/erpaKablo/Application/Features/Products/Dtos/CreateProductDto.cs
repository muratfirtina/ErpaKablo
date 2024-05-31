namespace Application.Features.Products.Dtos;

public class CreateProductDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string CategoryId { get; set; }
    public string BrandId { get; set; }
    public string Sku { get; set; }
    public List<ProductVariantDto> Variants { get; set; }
}