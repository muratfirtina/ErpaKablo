using Application.Features.Orders.Dtos;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Core.Persistence.Repositories;
using Domain;
using Domain.Enum;

namespace Application.Repositories;

public interface IOrderRepository: IAsyncRepository<Order, string>, IRepository<Order, string>
{
    Task<(bool, OrderDto)> ConvertCartToOrderAsync();
    Task<bool> CompleteOrderAsync(string orderId);
    //kullanının siparişlerini getir.
    Task<IPaginate<Order>> GetOrdersByUserAsync(PageRequest pageRequest, OrderStatus orderStatus, string? dateRange, string? searchTerm);
}