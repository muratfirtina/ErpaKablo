using Core.Persistence.Repositories;

namespace Domain;

public class Order:Entity<string>
{
    //public Guid CustomerId { get; set; }
    public string Description { get; set; }
    public string Address { get; set; }
    public Cart Cart { get; set; }

    public string OrderCode { get; set; }
    //public ICollection<Product> Products { get; set; }
    //public Customer Customer { get; set; }
    public CompletedOrder CompletedOrder { get; set; }
}