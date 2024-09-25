using Core.Persistence.Repositories;

namespace Domain;

public class OrderItem : Entity<string>
{
    public string OrderId { get; set; }
    public Order Order { get; set; } // İlgili sipariş
    public string ProductId { get; set; } // Sipariş edilen ürün
    public Product Product { get; set; }
    public int Quantity { get; set; } // Sipariş edilen miktar
    public bool IsChecked { get; set; } // Ürünün seçili olup olmadığı
    public ICollection<ProductImageFile> ProductImageFiles { get; set; } // Ürünün resimleri
        
    public OrderItem() : base("OrderItem")
    {
        ProductImageFiles = new List<ProductImageFile>();
    }
}