namespace Application.Features.Products.Dtos;

public class CreateProductVariantDto
{
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public List<VariantFeatureDto> VariantFeatures { get; set; }
}

//todo: sil