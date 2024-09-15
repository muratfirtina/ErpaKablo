using Domain.Enum;

namespace Application.Features.Products.Dtos.FilterDto;

public class FilterDefinitionDto
{
    public string Key { get; set; }
    public string DisplayName { get; set; }
    public FilterType Type { get; set; }
    public List<string> Options { get; set; }
}