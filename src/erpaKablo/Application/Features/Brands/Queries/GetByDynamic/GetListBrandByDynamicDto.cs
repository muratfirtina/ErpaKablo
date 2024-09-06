using Application.Features.Brands.Dtos;

namespace Application.Features.Brands.Queries.GetByDynamic;

public class GetListBrandByDynamicDto
{
    public string Id { get; set; } 
    public string Name { get; set; }
    public BrandImageFileDto? BrandImage { get; set; }

}