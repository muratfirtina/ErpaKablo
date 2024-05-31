using Application.Features.Products.Dtos;
using Core.Application.Responses;

namespace Application.Features.Products.Commands.Create;

public class CreatedProductResponse : IResponse
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string CategoryId { get; set; }
    public string BrandId { get; set; }
}