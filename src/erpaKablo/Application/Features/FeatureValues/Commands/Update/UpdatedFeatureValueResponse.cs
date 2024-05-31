using Core.Application.Responses;

namespace Application.Features.FeatureValues.Commands.Update;

public class UpdatedFeatureValueResponse : IResponse
{
    public string Id { get; set; }
    public string Value { get; set; }
}