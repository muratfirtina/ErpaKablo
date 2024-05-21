namespace Application.Features.Categories.Dtos;

public class GetListSubCategoryDto
{
    public GetListSubCategoryDto(List<GetListSubCategoryDto>? subCategories)
    {
        SubCategories = subCategories;
    }

    public string? Id { get; set; }
    public string? Name { get; set; }
    public List<GetListSubCategoryDto>? SubCategories { get; set; }
}