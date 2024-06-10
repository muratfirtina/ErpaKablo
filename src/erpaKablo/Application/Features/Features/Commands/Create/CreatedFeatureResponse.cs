using Application.Features.Categories.Dtos;
using Core.Application.Responses;

namespace Application.Features.Features.Commands.Create;

public class CreatedFeatureResponse : IResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public List<CategoryDto> Categories { get; set; }
}