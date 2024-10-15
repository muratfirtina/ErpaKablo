using Application.Features.Carts.Dtos;
using Domain;

namespace Application.Services;

public interface ICartService
{
    
    public Task<List<CartItem?>> GetCartItemsAsync();
    public Task AddItemToCartAsync(CreateCartItemDto cartItem);
    public Task UpdateQuantityAsync(UpdateCartItemDto cartItem);
    public Task RemoveCartItemAsync(string cartItemId);
    public Task UpdateCartItemAsync(UpdateCartItemDto cartItem);
    public Task<Cart?> GetUserActiveCart();
    Task<bool>RemoveCartAsync(string cartId);
    

}