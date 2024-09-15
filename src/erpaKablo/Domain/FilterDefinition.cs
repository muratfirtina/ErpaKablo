using Domain.Enum;

namespace Domain;

public class FilterDefinition
{
    public string Key { get; set; }
    public string DisplayName { get; set; }
    public FilterType Type { get; set; }
    public List<string> Options { get; set; }
}