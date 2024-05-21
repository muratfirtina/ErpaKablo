using Application.Features.Products.Dtos;

namespace Application.Features.Products.Queries.GetById;

public class GetByIdProductResponse
{
    public string Name { get; set; }
    public int? Stock { get; set; }
    public float? Price { get; set; }
    public string? Description { get; set; }
    public string? CategoryName { get; set; }
    public string? BrandName { get; set; }
    public ICollection<ProductFeatureDto> ProductFeatures { get; set; }
}