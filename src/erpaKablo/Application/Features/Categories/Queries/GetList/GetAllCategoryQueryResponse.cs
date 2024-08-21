using Application.Features.Categories.Dtos;
using Domain;

namespace Application.Features.Categories.Queries.GetList;

public class GetAllCategoryQueryResponse
{
    public string Id { get; set; } 
    public string Name { get; set; }
    public string? Title { get; set; }
    public string? ParentCategoryId { get; set; }
    public ICollection<GetListSubCategoryDto>? SubCategories { get; set; }
    public CategoryImageFileDto? CategoryImage { get; set; }
}