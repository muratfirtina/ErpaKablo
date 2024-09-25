namespace Domain.Enum;

public enum OrderStatus
{
    Pending,    // Sipariş beklemede
    Processing, // Sipariş hazırlanıyor
    Confirmed,  // Sipariş onaylandı
    Rejected,   // Sipariş reddedildi
    Delivered,  // Sipariş teslim edildi
    Completed,  // Sipariş tamamlandı
    Shipped,    // Sipariş gönderildi
    Cancelled,  // Sipariş iptal edildi
    Refunded    // Sipariş iade edildi
}