using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IOrderItemRepository : IAsyncRepository<OrderItem, string>, IRepository<OrderItem, string>
{
    Task<bool> RemoveOrderItemAsync(string? orderItemId);

    Task<bool> UpdateOrderItemQuantityAsync(string orderItemId, int newQuantity);
}