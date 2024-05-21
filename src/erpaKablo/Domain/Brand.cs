using Core.Persistence.Repositories;

namespace Domain;

public class Brand : Entity<string>
{
    public string? Name { get; set; }
    public ICollection<Product>? Products { get; set; }
    
    public Brand(string? name) : base(name)
    {
        Name = name;
    }
}