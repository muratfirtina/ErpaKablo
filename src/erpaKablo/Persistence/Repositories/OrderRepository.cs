using System.Globalization;
using Application.Extensions;
using Application.Features.Orders.Dtos;
using Application.Repositories;
using Application.Services;
using Application.Storage;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Core.Persistence.Repositories;
using Domain;
using Domain.Enum;
using Domain.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class OrderRepository:EfRepositoryBase<Order,string,ErpaKabloDbContext>,IOrderRepository
{
    private readonly ICartService _cartService;
    private readonly IProductRepository _productRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<AppUser> _userManager;
    private readonly IStorageService _storageService;
    private readonly IOrderItemRepository _orderItemRepository;
    public OrderRepository(ErpaKabloDbContext context, IProductRepository productRepository, ICartService cartService, IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager, IStorageService storageService, IOrderItemRepository orderItemRepository) : base(context)
    {
        _productRepository = productRepository;
        _cartService = cartService;
        _httpContextAccessor = httpContextAccessor;
        _userManager = userManager;
        _storageService = storageService;
        _orderItemRepository = orderItemRepository;
    }
    
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

    public async Task<string> ConvertCartToOrderAsync()
    {
        // 1. Kullanıcının aktif sepetini al
        Cart? activeCart = await _cartService.GetUserActiveCart();
        if (activeCart == null || !activeCart.CartItems.Any())
            throw new Exception("Aktif sepet bulunamadı veya boş.");

        // 2. Seçili ürünleri ve stokları kontrol et
        var selectedItems = activeCart.CartItems.Where(item => item.IsChecked).ToList();
        if (!selectedItems.Any())
            throw new Exception("Sepette seçili ürün yok.");

        foreach (var cartItem in selectedItems)
        {
            var product = await _productRepository.GetAsync(p => p.Id == cartItem.ProductId);
            if (product == null)
                throw new Exception($"Ürün bulunamadı: {cartItem.ProductId}");

            if (cartItem.Quantity > product.Stock)
                throw new Exception($"Yeterli stok yok: {product.Name}, mevcut stok: {product.Stock}");
        }

        // Kullanıcının varsayılan adresini al
        var user = await _userManager.Users.Include(u => u.UserAddresses)
            .FirstOrDefaultAsync(u => u.Id == activeCart.UserId);
        if (user == null)
            throw new Exception("Kullanıcı bulunamadı.");

        var defaultAddress = user.UserAddresses.FirstOrDefault(a => a.IsDefault);
        if (defaultAddress == null)
            throw new Exception("Varsayılan adres bulunamadı.");

        // Sipariş kodu üretimi (benzersiz)
        var orderCode = (new Random().NextDouble() * 10000).ToString(CultureInfo.InvariantCulture);
        orderCode = orderCode.Substring(orderCode.IndexOf(".", StringComparison.Ordinal) + 1,
            orderCode.Length - orderCode.IndexOf(".", StringComparison.Ordinal) - 1);

        // 3. Siparişi oluştur
        var order = new Order
        {
            UserId = activeCart.UserId,
            User = activeCart.User,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            OrderCode = orderCode,
            UserAddressId = defaultAddress.Id,
            OrderItems = selectedItems.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Product = item.Product,
                Quantity = item.Quantity,
                IsChecked = true
            }).ToList(),
            TotalPrice = selectedItems.Sum(item => item.Product.Price * item.Quantity)
        };

        await AddAsync(order); // Siparişi kaydet

        // 4. Seçili olmayan ürünler için yeni bir sepet oluştur
        var unselectedItems = activeCart.CartItems.Where(item => !item.IsChecked).ToList();
        if (unselectedItems.Any())
        {
            var newCart = new Cart
            {
                UserId = activeCart.UserId,
                User = activeCart.User,
                CartItems = unselectedItems
            };

            // Yeni sepeti veritabanına ekle
            await Context.Set<Cart>().AddAsync(newCart);
        }

        // 5. Eski sepeti sil (aktif olan sepet)
        await _cartService.RemoveCartAsync(activeCart.Id);

        // 6. Stoğu güncelle
        foreach (var cartItem in selectedItems)
        {
            var product = await _productRepository.GetAsync(p => p.Id == cartItem.ProductId);
            if (product != null)
            {
                product.Stock -= cartItem.Quantity; // Stok güncelle
                await _productRepository.UpdateAsync(product); // Ürünü güncelle
            }
        }

        await UpdateAsync(order); // Değişiklikleri kaydet

        return order.Id; // Sipariş ID'sini döndür
    }

    public async Task<GetListResponse<OrderDto>> GetUserOrdersAsync(PageRequest pageRequest)
    {
        // Aktif kullanıcıyı al
        AppUser? currentUser = await GetCurrentUserAsync();
        if (currentUser == null)
            throw new Exception("User not found.");
        
        // Kullanıcının siparişlerini En yeni siparişler önce gelecek şekilde getir.
        IPaginate<Order> orders = await GetListAsync(
            predicate: o => o.UserId == currentUser.Id,
            include: o => o.Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.ProductImageFiles.Where(pif => pif.Showcase))
                .Include(o => o.User),
            orderBy:o=>o.OrderByDescending(o=>o.OrderDate)
        );
        
        var orderDtos = orders.Items.Select(o => new OrderDto
        {
            OrderId = o.Id,
            OrderDate = o.OrderDate,
            TotalPrice = o.TotalPrice,
            OrderItems = o.OrderItems.Select(oi => new OrderItemDto
            {
                //BrandName = oi.Product.Brand.Name,
                ProductName = oi.Product.Name,
                Quantity = oi.Quantity,
                Price = oi.Product.Price,
                ShowcaseImage = oi.ProductImageFiles.SetImageUrl(_storageService)
            }).ToList()
        }).ToList();
        
        return new GetListResponse<OrderDto>
        {
            Items = orderDtos,
            Pages = orders.Pages,
            Size = orders.Size,
            Count = orders.Count,
            HasNext = orders.HasNext,
            HasPrevious = orders.HasPrevious
        };

    }
    
    public async Task<bool> CompleteOrderAsync(string orderId)
    {
        // 1. Siparişi veritabanından sorgula
        var order = await Query()
            .FirstOrDefaultAsync(o => o.Id == orderId);

        if (order == null)
        {
            throw new Exception("Order not found.");
        }

        // 2. Siparişin durumunu güncelle (OrderStatus.Confirmed)
        order.Status = OrderStatus.Confirmed;

        // 3. Siparişi güncelle ve kaydet
        await UpdateAsync(order);
        return true;
    }
    
}

