namespace Application.Features.Products.Dtos;

public class CreateProductDto
{
    public string Name { get; set; }
    public string CategoryId { get; set; }
    public string BrandId { get; set; }
    public string VaryantGroupID { get; set; }
    public string Sku { get; set; }
    public decimal? Price { get; set; }
    public int? Stock { get; set; }
    public string? Tax { get; set; }
    public List<ProductFeatureValueDto> ProductFeatureValues { get; set; }

}