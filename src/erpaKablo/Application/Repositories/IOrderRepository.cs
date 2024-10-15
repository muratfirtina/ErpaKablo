using Application.Features.Orders.Dtos;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IOrderRepository: IAsyncRepository<Order, string>, IRepository<Order, string>
{
    Task<string> ConvertCartToOrderAsync();
    Task<GetListResponse<OrderDto>> GetUserOrdersAsync(PageRequest pageRequest);
    Task<bool> CompleteOrderAsync(string orderId);
    
}