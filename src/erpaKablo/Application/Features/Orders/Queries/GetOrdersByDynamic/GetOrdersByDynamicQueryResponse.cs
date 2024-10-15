using Core.Application.Responses;
using Domain.Enum;

namespace Application.Features.Orders.Queries.GetOrdersByDynamic;

public class GetOrdersByDynamicQueryResponse : IResponse
{
    //public string OrderId { get; set; }
    public string OrderCode { get; set; }
   // public string UserId { get; set; }
    public string UserName { get; set; }
    public DateTime OrderDate { get; set; }
    public decimal? TotalPrice { get; set; }
    public OrderStatus Status { get; set; }
}