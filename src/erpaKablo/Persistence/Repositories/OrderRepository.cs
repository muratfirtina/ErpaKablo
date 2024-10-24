using System.Globalization;
using Application.Extensions;
using Application.Features.Orders.Dtos;
using Application.Features.Products.Dtos;
using Application.Repositories;
using Application.Services;
using Application.Storage;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Core.Persistence.Repositories;
using Domain;
using Domain.Enum;
using Domain.Identity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Persistence.Context;

namespace Persistence.Repositories;

public class OrderRepository : EfRepositoryBase<Order, string, ErpaKabloDbContext>, IOrderRepository
{
    private readonly ICartService _cartService;
    private readonly IProductRepository _productRepository;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly UserManager<AppUser> _userManager;
    private readonly IStorageService _storageService;
    private readonly IOrderItemRepository _orderItemRepository;

    public OrderRepository(ErpaKabloDbContext context, IProductRepository productRepository, ICartService cartService,
        IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager, IStorageService storageService,
        IOrderItemRepository orderItemRepository) : base(context)
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

    public async Task<(bool, OrderDto)> ConvertCartToOrderAsync()
    {
        try
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
                // Seçili ürünlerin bilgilerini OrderItem'a sabitle
                OrderItems = selectedItems.Select(item => new OrderItem
                {
                    ProductId = item.ProductId, // Ürünün ID'si
                    ProductName = item.Product.Name, // Ürün adı
                    ProductTitle = item.Product.Title, // Ürün başlığı
                    Price = item.Product.Price, // Ürün fiyatını sabitle
                    BrandName = item.Product.Brand?.Name, // Ürün markasını sabitle
                    Quantity = item.Quantity, // Miktar
                    ProductFeatureValues = item.Product.ProductFeatureValues.Select(fv => new ProductFeatureValue
                    {
                        FeatureValue = fv.FeatureValue,
                        FeatureValueId = fv.FeatureValueId
                    }).ToList(),
                    // Resimleri sabitlemek istemediğiniz için ProductImageFiles listesi dahil edilmiyor
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

            // 7. Sipariş detaylarını içeren OrderDto oluştur
            var orderDto = new OrderDto
            {
                OrderId = order.Id,
                OrderCode = order.OrderCode,
                UserName = order.User.UserName,
                Email = user.Email,
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                UserAddress = order.UserAddress,
                OrderItems = order.OrderItems.Select(oi => new OrderItemDto
                {
                    ProductName = oi.ProductName,
                    Quantity = oi.Quantity,
                    Price = oi.Price, // Sabitlenmiş fiyat
                    ProductTitle = oi.ProductTitle,
                    BrandName = oi.BrandName,
                    ProductFeatureValues = oi.ProductFeatureValues.Select(fv => new ProductFeatureValueDto
                    {
                        FeatureName = fv.FeatureValue.Feature.Name,
                        FeatureValueName = fv.FeatureValue.Name
                    }).ToList(),
                    ShowcaseImage = oi.Product.ProductImageFiles.FirstOrDefault(img => img.Showcase) // Dinamik resim
                        ?.SetImageUrl(_storageService)
                }).ToList()
            };

            return (true, orderDto); // Başarıyla sipariş oluşturuldu, orderDto döndürülüyor
        }
        catch (Exception ex)
        {
            // Hata durumunda false döndürülüyor ve boş bir OrderDto dönüyor
            return (false, null);
        }
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

    public async Task<IPaginate<Order>> GetOrdersByUserAsync(PageRequest pageRequest, OrderStatus orderStatus,
    string? dateRange, string? searchTerm) // searchTerm nullable yapılmış
{
    AppUser? user = await GetCurrentUserAsync();
    if (user == null)
    {
        throw new Exception("User not found.");
    }

    var query = Context.Orders.AsQueryable();

    // Kullanıcıya göre filtrele
    query = query.Where(o => o.UserId == user.Id); // Kullanıcıya ait siparişleri filtreleme

    // Eğer searchTerm boş değilse arama yap
    if (!string.IsNullOrWhiteSpace(searchTerm))
    {
        var terms = searchTerm.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        foreach (var term in terms)
        {
            var termParam = term.ToLower();
            query = query.Where(o =>
                EF.Functions.Like(o.Description.ToLower(), $"%{termParam}%") ||
                EF.Functions.Like(o.OrderCode.ToLower(), $"%{termParam}%") ||
                EF.Functions.Like(o.OrderItems.Select(oi => oi.ProductName).FirstOrDefault().ToLower(),
                    $"%{termParam}%") ||
                EF.Functions.Like(o.OrderItems.Select(oi => oi.ProductTitle).FirstOrDefault().ToLower(),
                    $"%{termParam}%"));
        }
    }

    // OrderStatus'a göre filtreleme
    if (orderStatus != OrderStatus.All)
    {
        query = query.Where(o => o.Status == orderStatus);
    }

    // Tarih aralığına göre filtreleme
    if (!string.IsNullOrWhiteSpace(dateRange))
    {
        var dates = dateRange.Split('-');
        if (dates.Length == 2)
        {
            var startDate = DateTime.Parse(dates[0]);
            var endDate = DateTime.Parse(dates[1]);
            query = query.Where(o => o.OrderDate >= startDate && o.OrderDate <= endDate);
        }
    }

    // Sonuçları sıralayıp gerekli ilişkileri dahil ederek getir
    query = query
        .OrderByDescending(o => o.OrderDate)
        .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product)
        .ThenInclude(p => p.ProductImageFiles)
        .Include(o => o.OrderItems)
        .ThenInclude(oi => oi.Product)
        .ThenInclude(p => p.Brand)
        .Include(o => o.User);

    return await query.ToPaginateAsync(pageRequest.PageIndex, pageRequest.PageSize);
}
    /*
    public async Task<IPaginate<Order>> GetOrdersByUserAsync(PageRequest pageRequest, DynamicQuery dynamicQuery)
    {
        // Kullanıcıyı çekiyoruz
        AppUser? user = await GetCurrentUserAsync();
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        // Kullanıcının siparişlerini sorguluyoruz ve UserAddress'i dahil ediyoruz
        IPaginate<Order> orders = await GetListByDynamicAsync(
            dynamicQuery,
            predicate: o => o.UserId == user.Id,
            include: o => o.Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.ProductImageFiles) // ShowcaseImage için
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product)
                .ThenInclude(p => p.Brand)
                .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.Product).ThenInclude(p => p.ProductFeatureValues).ThenInclude(pfv => pfv.FeatureValue).ThenInclude(fv => fv.Feature) // BrandName için
                .Include(o => o.User), // UserAddress bilgilerini ekledik
            index: pageRequest.PageIndex,
            size: pageRequest.PageSize
        );

        return orders; // Siparişleri döndürüyoruz
    }
    */
}