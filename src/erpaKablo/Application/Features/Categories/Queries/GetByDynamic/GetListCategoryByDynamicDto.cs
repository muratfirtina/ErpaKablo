using Application.Features.Products.Dtos;

namespace Application.Features.Categories.Queries.GetByDynamic;

public class GetListCategoryByDynamicDto
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<GetListCategoryByDynamicDto> SubCategories { get; set; } = new List<GetListCategoryByDynamicDto>();
    public List<ProductDto> Products { get; set; } = new List<ProductDto>();
}