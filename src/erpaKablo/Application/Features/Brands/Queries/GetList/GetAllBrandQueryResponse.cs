using Application.Features.Brands.Dtos;

namespace Application.Features.Brands.Queries.GetList;

public class GetAllBrandQueryResponse
{
    public string Id { get; set; } 
    public string Name { get; set; }
    public BrandImageFileDto? BrandImage { get; set; }
}