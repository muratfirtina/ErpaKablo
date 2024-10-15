using Application.Extensions;
using Application.Features.Orders.Dtos;
using Application.Features.ProductImageFiles.Dtos;
using Application.Features.Products.Dtos;
using Core.Application.Responses;
using Domain;
using Domain.Enum;

namespace Application.Features.Orders.Queries.GetById;

public class GetOrderByIdQueryResponse :IResponse
{
    public string Id { get; set; }
    public DateTime OrderDate { get; set; }
    public string OrderCode { get; set; }
    public decimal? TotalPrice { get; set; }
    public OrderStatus Status { get; set; }
    public List<OrderItemDto> OrderItems { get; set; } = new List<OrderItemDto>();
    public string UserName { get; set; }
    public UserAddress UserAddress { get; set; }
    public string Description { get; set; }
    public string PhoneNumber { get; set; }
   
}