using Domain;

namespace Application.Features.Products.Dtos;

public class ProductFeatureDto
{
    public string? ProductFeatureGroupName { get; set; }
    public List<FeatureDto>? FeatureDetails { get; set; }
}