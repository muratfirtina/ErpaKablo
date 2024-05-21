using Core.Application.Responses;

namespace Application.Features.ProductFeatures.Commands.Update;

public class UpdatedProductFeatureResponse : IResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
}