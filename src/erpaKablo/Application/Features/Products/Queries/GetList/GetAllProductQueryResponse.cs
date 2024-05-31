using Application.Features.Products.Dtos;

namespace Application.Features.Products.Queries.GetList;

public class GetAllProductQueryResponse
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string BrandId { get; set; }
    public string BrandName { get; set; }
    public string Sku { get; set; }
    public List<ProductVariantDto> Variants { get; set; }
    
}