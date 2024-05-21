using Application.Features.Categories.Dtos;

namespace Application.Features.Categories.Queries.GetById;

public class GetByIdCategoryResponse
{
    public string Name { get; set; }
    public string? ParentCategoryId { get; set; }
    public string? ParentCategoryName { get; set; }
    
    public ICollection<GetListSubCategoryDto>? SubCategories { get; set; }
}