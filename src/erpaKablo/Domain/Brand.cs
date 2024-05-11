using Core.Persistence.Repositories;

namespace Domain;

public class Brand : Entity<int>
{
    public string? Name { get; set; }
    public ICollection<Product>? Products { get; set; }
}