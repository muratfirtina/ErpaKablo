using Application.Features.Carts.Dtos;
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
    Task<(bool success, OrderDto? orderDto, List<CartItemDto>? newCartItems)> ConvertCartToOrderAsync(string? addressId,
        string? phoneNumberId,
        string? description);
    Task<bool> CompleteOrderAsync(string orderId);
    
    //GetUserOrderByIdAsync
    Task<Order> GetUserOrderByIdAsync(string orderId);
    
    Task<IPaginate<Order>> GetOrdersByUserAsync(PageRequest pageRequest, OrderStatus orderStatus, string? dateRange, string? searchTerm);
    Task<bool> UpdateOrderWithAdminNotesAsync(
        string orderId, 
        string? adminNote, 
        string? adminUserName,
        List<(string OrderItemId, decimal? UpdatedPrice, int? LeadTime)> itemUpdates);
}