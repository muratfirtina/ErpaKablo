using Core.Application.Responses;

namespace Application.Features.Products.Commands.Update;

public class UpdateProductResponse : IResponse
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string CategoryName { get; set; }
    public string BrandName { get; set; }
    public string Description { get; set; }
}