using Application.Features.Carts.Dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Application.Repositories;
using Application.Services;
using Domain;
using Domain.Enum;
using Domain.Identity;


namespace Persistence.Services;

public class CartService : ICartService
{
    private readonly IProductRepository _productRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<AppUser> _userManager;
    private readonly ICartRepository _cartRepository;
    private readonly ICartItemRepository _cartItemRepository;

    public CartService(
        IHttpContextAccessor httpContextAccessor,
        UserManager<AppUser> userManager,
        ICartRepository cartRepository,
        ICartItemRepository cartItemRepository,
        IProductRepository productRepository)
    {
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _cartRepository = cartRepository;
        _cartItemRepository = cartItemRepository;
        _productRepository = productRepository;
    }

    // 1. Kullanıcıyı doğrulayan metod
    private async Task<AppUser?> GetCurrentUserAsync()
    {
        var userName = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
        if (!string.IsNullOrEmpty(userName))
        {
            AppUser? user = await _userManager.Users
                .FirstOrDefaultAsync(u => u.UserName == userName);

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            return user;
        }
        throw new Exception("Unexpected error occurred.");
    }

    // 2. Sepeti bulan veya oluşturan metod
    private async Task<Cart> GetOrCreateCartAsync(AppUser user)
    {
        var cartWithoutOrder = await _cartRepository.GetAsync(
            predicate: c => c.UserId == user.Id && c.Order == null,
            include: c => c.Include(c => c.Order)
        );

        if (cartWithoutOrder == null)
        {
            cartWithoutOrder = new Cart { UserId = user.Id };
            await _cartRepository.AddAsync(cartWithoutOrder);
        }

        return cartWithoutOrder;
    }

    public async Task<List<CartItem?>> GetCartItemsAsync()
    {
        AppUser? user = await GetCurrentUserAsync();
        Cart? cart = await GetOrCreateCartAsync(user);
        var result = await _cartRepository.GetAsync(
            predicate: c => c.Id == cart.Id,
            include: c => c
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.ProductImageFiles)
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.Brand)
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.ProductFeatureValues).ThenInclude(x => x.FeatureValue).ThenInclude(x => x.Feature)
        );

        return result?.CartItems?.ToList() ?? new List<CartItem?>();
    }

    public async Task AddItemToCartAsync(CreateCartItemDto cartItem)
    {
        AppUser? user = await GetCurrentUserAsync();
        Cart? cart = await GetOrCreateCartAsync(user);

        var product = await _productRepository.GetAsync(predicate: p => p.Id == cartItem.ProductId);
        if (product.Stock <= 0)
        {
            throw new Exception("Product stock is not enough.");
        }

        var _cartItem = await _cartItemRepository.GetAsync(
            predicate: ci => ci.CartId == cart.Id && ci.ProductId == cartItem.ProductId);

        if (_cartItem != null)
        {
            if (_cartItem.Quantity < product.Stock)
            {
                _cartItem.Quantity++;
                if (!cartItem.IsChecked)
                    _cartItem.IsChecked = false;
                await _cartItemRepository.UpdateAsync(_cartItem);
            }
            else
            {
                throw new Exception("Product stock is not enough.");
            }
        }
        else
        {
            await _cartItemRepository.AddAsync(new CartItem
            {
                CartId = cart.Id,
                ProductId = cartItem.ProductId,
                Quantity = 1,
                IsChecked = cartItem.IsChecked
            });
        }
    }

    public async Task UpdateQuantityAsync(UpdateCartItemDto cartItem)
    {
        var _cartItem = await _cartItemRepository.GetAsync(predicate: ci => ci.Id == cartItem.CartItemId);
        if (_cartItem == null)
            throw new Exception("Cart item not found.");

        var product = await _productRepository.GetAsync(predicate: p => p.Id == _cartItem.ProductId);
        if (product == null)
            throw new Exception("Product not found.");

        if (cartItem.Quantity > product.Stock || cartItem.Quantity < 0)
            throw new Exception("Invalid quantity.");

        if (cartItem.Quantity == 0)
        {
            await _cartItemRepository.DeleteAsync(_cartItem);
        }
        else
        {
            _cartItem.Quantity = cartItem.Quantity;
            await _cartItemRepository.UpdateAsync(_cartItem);
        }
    }

    public async Task RemoveCartItemAsync(string cartItemId)
    {
        var cartItem = await _cartItemRepository.GetAsync(predicate: ci => ci.Id == cartItemId);
        if (cartItem != null)
        {
            await _cartItemRepository.DeleteAsync(cartItem);
        }
    }

    public async Task UpdateCartItemAsync(UpdateCartItemDto cartItem)
    {
        var _cartItem = await _cartItemRepository.GetAsync(predicate: ci => ci.Id == cartItem.CartItemId);
        if (_cartItem != null)
        {
            _cartItem.IsChecked = cartItem.IsChecked;
            await _cartItemRepository.UpdateAsync(_cartItem);
        }
    }

    public async Task<Cart?> GetUserActiveCart()
    {
        AppUser? user = await GetCurrentUserAsync();
        Cart? cart = await GetOrCreateCartAsync(user);
        
        return await _cartRepository.GetAsync(
            predicate: c => c.Id == cart.Id,
            include: c => c
                .Include(c => c.CartItems)
                .ThenInclude(ci => ci.Product)
                .ThenInclude(p => p.ProductImageFiles)
        );
    }

    public async Task<bool> RemoveCartAsync(string cartId)
    {
        var cart = await _cartRepository.GetAsync(predicate: c => c.Id == cartId);
        if (cart != null)
        {
            await _cartRepository.DeleteAsync(cart);
            return true;
        }
        return false;
    }
}