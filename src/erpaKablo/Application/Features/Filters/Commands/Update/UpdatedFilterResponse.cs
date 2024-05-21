using Core.Application.Responses;

namespace Application.Features.Filters.Commands.Update;

public class UpdatedFilterResponse : IResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
}