using Core.Application.Responses;

namespace Application.Features.Categories.Commands.Update;

public class UpdatedCategoryResponse : IResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    
}