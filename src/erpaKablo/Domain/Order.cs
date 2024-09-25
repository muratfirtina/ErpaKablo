using Core.Persistence.Repositories;
using Domain.Enum;
using Domain.Identity;

namespace Domain;

public class Order:Entity<string>
{
    //public Guid CustomerId { get; set; }
    public string UserId { get; set; }
    public AppUser User { get; set; } // Siparişi veren kullanıcı
    public DateTime OrderDate { get; set; }
    public decimal TotalPrice { get; set; } // Toplam fiyat
    public OrderStatus Status { get; set; }  // Sipariş durumu (Pending, Completed vb.)
    public ICollection<OrderItem> OrderItems { get; set; } // Sipariş öğeleri
    public string UserAddressId { get; set; }
    public UserAddress UserAddress { get; set; }
    public string? Description { get; set; }

    public string OrderCode { get; set; }
    //public ICollection<Product> Products { get; set; }
    //public Customer Customer { get; set; }
    public CompletedOrder CompletedOrder { get; set; }
    public Order() : base("Order")
    {
        OrderItems = new List<OrderItem>();
    }
}