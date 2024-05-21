namespace Application.Features.CategoryFilters.Queries.GetList;

public class GetAllCategoryFilterQueryResponse
{
    public string Id { get; set; } 
    public string Name { get; set; }
    public string CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string FilterId { get; set; }
    public string FilterName { get; set; }
}