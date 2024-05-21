using Core.Application.Responses;

namespace Application.Features.CategoryFilters.Commands.Update;

public class UpdatedCategoryFilterResponse : IResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
}