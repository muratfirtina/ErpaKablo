using System.Globalization;
using Application.Extensions;
using Application.Features.Carts.Dtos;
using Application.Features.Orders.Dtos;
using Application.Features.PhoneNumbers.Dtos;
using Application.Features.ProductImageFiles.Dtos;
using Application.Features.Products.Dtos;
using Application.Features.UserAddresses.Dtos;
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

    public async Task<(bool success, OrderDto? orderDto, List<CartItemDto>? newCartItems)> ConvertCartToOrderAsync(string addressId, string phoneNumberId, string description)
{
   try
   {
       // 1. Kullanıcının aktif sepetini al
       Cart? activeCart = await _cartService.GetUserActiveCart();
       if (activeCart == null || !activeCart.CartItems.Any())
           throw new Exception("Aktif sepet bulunamadı veya boş.");

       // 2. Seçili olmayan ürünler için yeni sepet oluştur
       AppUser? user = await _userManager.Users
           .Include(u => u.UserAddresses)
           .Include(u => u.PhoneNumbers)
           .FirstOrDefaultAsync(u => u.Id == activeCart.UserId);

       if (user == null)
           throw new Exception("Kullanıcı bulunamadı.");

       var newCart = new Cart { UserId = user.Id, User = user };
       var uncheckedItems = activeCart.CartItems.Where(item => !item.IsChecked).ToList();

       // Yeni sepeti kaydet
       if (uncheckedItems.Any())
       {
           await Context.Carts.AddAsync(newCart);
           await Context.SaveChangesAsync();

           // İşaretlenmemiş ürünlerin CartId'lerini yeni sepete güncelle
           foreach (var item in uncheckedItems)
           {
               item.CartId = newCart.Id;
               Context.CartItems.Update(item);
           }
           await Context.SaveChangesAsync();
       }

       // 3. Seçili ürünleri ve stokları kontrol et
       var selectedItems = activeCart.CartItems.Where(item => item.IsChecked).ToList();
       if (!selectedItems.Any())
           throw new Exception("Sepette seçili ürün yok.");

       // Stok kontrolü
       foreach (var cartItem in selectedItems)
       {
           var product = await _productRepository.GetAsync(p => p.Id == cartItem.ProductId);
           if (product == null)
               throw new Exception("Ürün bulunamadı.");

           if (product.Stock < cartItem.Quantity)
               throw new Exception($"{product.Name} ürününden stokta yeterli miktarda bulunmamaktadır.");
       }

       var selectedAddress = user.UserAddresses.FirstOrDefault(a => a.Id == addressId);
       if (selectedAddress == null)
           throw new Exception("Seçilen adres bulunamadı.");

       var selectedPhone = user.PhoneNumbers.FirstOrDefault(p => p.Id == phoneNumberId);
       if (selectedPhone == null)
           throw new Exception("Seçilen telefon numarası bulunamadı.");

       // 4. Siparişi oluştur
       var order = new Order
       {
           UserId = activeCart.UserId,
           User = activeCart.User,
           OrderDate = DateTime.UtcNow,
           Status = OrderStatus.Pending,
           OrderCode = GenerateOrderCode(),
           UserAddressId = selectedAddress.Id,
           PhoneNumberId = selectedPhone.Id,
           Description = description,
           OrderItems = selectedItems.Select(item => new OrderItem
           {
               ProductId = item.ProductId,
               ProductName = item.Product.Name,
               ProductTitle = item.Product.Title,
               Price = item.Product.Price,
               BrandName = item.Product.Brand?.Name,
               Quantity = item.Quantity,
               ProductFeatureValues = item.Product.ProductFeatureValues.Select(fv => new ProductFeatureValue
               {
                   FeatureValue = fv.FeatureValue,
                   FeatureValueId = fv.FeatureValueId
               }).ToList(),
               IsChecked = true
           }).ToList(),
           TotalPrice = selectedItems.Sum(item => item.Product.Price * item.Quantity)
       };

       await AddAsync(order);

       // 5. Eski sepeti sil
       await _cartService.RemoveCartAsync(activeCart.Id);

       // 6. Stoğu güncelle
       foreach (var cartItem in selectedItems)
       {
           var product = await _productRepository.GetAsync(p => p.Id == cartItem.ProductId);
           if (product != null)
           {
               product.Stock -= cartItem.Quantity;
               await _productRepository.UpdateAsync(product);
           }
       }

       await UpdateAsync(order);

       // 7. OrderDto oluştur
       var orderDto = new OrderDto
       {
           OrderId = order.Id,
           OrderCode = order.OrderCode,
           UserName = order.User.UserName,
           Email = user.Email,
           OrderDate = order.OrderDate,
           TotalPrice = order.TotalPrice,
           UserAddress = new UserAddressDto
           {
               Id = selectedAddress.Id,
               Name = selectedAddress.Name,
               AddressLine1 = selectedAddress.AddressLine1,
               AddressLine2 = selectedAddress.AddressLine2,
               State = selectedAddress.State,
               City = selectedAddress.City,
               Country = selectedAddress.Country,
               PostalCode = selectedAddress.PostalCode,
               IsDefault = selectedAddress.IsDefault
           },
           PhoneNumber = new PhoneNumberDto
           {
               Name = selectedPhone.Name,
               Number = selectedPhone.Number,
           },
           Description = order.Description,
           OrderItems = order.OrderItems.Select(oi => new OrderItemDto
           {
               ProductName = oi.ProductName,
               Quantity = oi.Quantity,
               Price = oi.Price,
               ProductTitle = oi.ProductTitle,
               BrandName = oi.BrandName,
               ProductFeatureValues = oi.ProductFeatureValues.Select(fv => new ProductFeatureValueDto
               {
                   FeatureName = fv.FeatureValue.Feature.Name,
                   FeatureValueName = fv.FeatureValue.Name
               }).ToList(),
               ShowcaseImage = oi.Product.ProductImageFiles.FirstOrDefault(img => img.Showcase)?.SetImageUrl(_storageService)
           }).ToList()
       };

       // 8. İşaretlenmemiş ürünlerin CartItemDto listesini oluştur
       var newCartItems = uncheckedItems.Select(item => new CartItemDto
       {
           CartItemId = item.Id,
           CartId = newCart.Id,
           ProductId = item.ProductId,
           ProductName = item.Product.Name,
           ProductTitle = item.Product.Title,
           BrandName = item.Product.Brand?.Name,
           Quantity = item.Quantity,
           UnitPrice = item.Product.Price,
           IsChecked = item.IsChecked,
           ProductFeatureValues = item.Product.ProductFeatureValues.Select(fv => new ProductFeatureValueDto
           {
               FeatureName = fv.FeatureValue.Feature.Name,
               FeatureValueName = fv.FeatureValue.Name
           }).ToList(),
           ShowcaseImage = item.Product.ProductImageFiles.FirstOrDefault(img => img.Showcase)?.SetImageUrl(_storageService)
       }).ToList();

       return (true, orderDto, uncheckedItems.Any() ? newCartItems : null);
   }
   catch (Exception ex)
   {
       return (false, null, null);
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

    // Persistence/Repositories/OrderRepository.cs

    public async Task<Order> GetUserOrderByIdAsync(string orderId)
    {
        var user = await GetCurrentUserAsync();
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        var order = await Query()
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.ProductImageFiles)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.Brand)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.ProductFeatureValues)
            .ThenInclude(pfv => pfv.FeatureValue)
            .ThenInclude(fv => fv.Feature)
            .Include(o => o.User)
            .Include(o => o.UserAddress)
            .Include(o => o.PhoneNumber)
            .AsSplitQuery() 
            .FirstOrDefaultAsync(o => o.Id == orderId && o.UserId == user.Id);

        if (order == null)
            throw new Exception("Order not found or you don't have permission to view this order.");

        return order;
    }

    private string GenerateOrderCode()
    {
        var orderCode = (new Random().NextDouble() * 10000).ToString(CultureInfo.InvariantCulture);
        orderCode = orderCode.Substring(orderCode.IndexOf(".", StringComparison.Ordinal) + 1,
            orderCode.Length - orderCode.IndexOf(".", StringComparison.Ordinal) - 1);
        return orderCode;
    }

    public async Task<IPaginate<Order>> GetOrdersByUserAsync(PageRequest pageRequest, OrderStatus orderStatus,
        string? dateRange, string? searchTerm)
    {
        AppUser? user = await GetCurrentUserAsync();
        if (user == null)
        {
            throw new Exception("User not found.");
        }

        var query = Context.Orders.AsQueryable();

        // Kullanıcıya göre filtrele
        query = query.Where(o => o.UserId == user.Id);

        // Tarih filtreleme
        if (!string.IsNullOrWhiteSpace(dateRange))
        {
            var currentDate = DateTime.UtcNow;

            switch (dateRange)
            {
                case "30":
                    query = query.Where(o => o.OrderDate >= currentDate.AddDays(-30));
                    break;
                case "180":
                    query = query.Where(o => o.OrderDate >= currentDate.AddDays(-180));
                    break;
                case "365":
                    query = query.Where(o => o.OrderDate >= currentDate.AddDays(-365));
                    break;
                case "older1":
                    var oneYearAgo = currentDate.AddYears(-1);
                    var twoYearsAgo = currentDate.AddYears(-2);
                    query = query.Where(o => o.OrderDate <= oneYearAgo && o.OrderDate >= twoYearsAgo);
                    break;
                case "older2":
                    var twoYearsAgo2 = currentDate.AddYears(-2);
                    var threeYearsAgo = currentDate.AddYears(-3);
                    query = query.Where(o => o.OrderDate <= twoYearsAgo2 && o.OrderDate >= threeYearsAgo);
                    break;
                case "older3":
                    var threeYearsAgo2 = currentDate.AddYears(-3);
                    query = query.Where(o => o.OrderDate <= threeYearsAgo2);
                    break;
            }
        }

        // OrderStatus gruplarına göre filtreleme
        if (orderStatus != OrderStatus.All)
        {
            switch (orderStatus)
            {
                case OrderStatus.Processing: // Devam Edenler grubu
                    query = query.Where(o =>
                        o.Status == OrderStatus.Pending ||
                        o.Status == OrderStatus.Processing ||
                        o.Status == OrderStatus.Confirmed ||
                        o.Status == OrderStatus.Shipped);
                    break;
                case OrderStatus.Cancelled: // İptal Edilenler grubu
                    query = query.Where(o =>
                        o.Status == OrderStatus.Cancelled ||
                        o.Status == OrderStatus.Rejected);
                    break;
                case OrderStatus.Returned: // İade Edilenler grubu
                    query = query.Where(o =>
                        o.Status == OrderStatus.Returned);
                    break;
                case OrderStatus.Completed: // Tamamlananlar grubu
                    query = query.Where(o =>
                        o.Status == OrderStatus.Completed);
                    break;
            }
        }

        // Arama filtresi
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

        query = query
            .OrderByDescending(o => o.OrderDate)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.ProductImageFiles)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.Brand)
            .Include(o => o.OrderItems)
            .ThenInclude(oi => oi.Product)
            .ThenInclude(p => p.ProductFeatureValues)
            .ThenInclude(pfv => pfv.FeatureValue)
            .AsSplitQuery() 
            .Include(o => o.User);

        return await query.ToPaginateAsync(pageRequest.PageIndex, pageRequest.PageSize);
    }

    public async Task<bool> UpdateOrderWithAdminNotesAsync(
        string orderId, 
        string adminNote, 
        string adminUserName,
        List<(string OrderItemId, decimal? UpdatedPrice, int? LeadTime)> itemUpdates)
    {
        using var transaction = await Context.Database.BeginTransactionAsync();

        try
        {
            var order = await Query()
                .Include(o => o.OrderItems)
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null) throw new Exception("Order not found");

            // Update order details
            order.AdminNote = adminNote;
            order.LastModifiedBy = adminUserName;

            // Update order items
            foreach (var item in itemUpdates)
            {
                var orderItem = order.OrderItems.FirstOrDefault(oi => oi.Id == item.OrderItemId);
                if (orderItem != null)
                {
                    orderItem.UpdatedPrice = item.UpdatedPrice;
                    orderItem.LeadTime = item.LeadTime;
                    orderItem.PriceUpdateDate = DateTime.UtcNow;
                }
            }

            await UpdateAsync(order);

            // Send notification email
            /*if (order.User?.Email != null)
            {
                await _mailService.SendOrderUpdateNotificationAsync(
                    order.User.Email,
                    order.OrderCode,
                    adminNote,
                    order.OrderItems.ToList(),
                    order.TotalPrice);
            }*/

            await transaction.CommitAsync();
            return true;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}