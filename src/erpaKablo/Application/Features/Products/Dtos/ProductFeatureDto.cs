namespace Application.Features.Products.Dtos;

public class ProductFeatureDto
{
    public string? FeatureGroupName { get; set; }
    public ICollection<string>? FeatureDetails { get; set; }
}