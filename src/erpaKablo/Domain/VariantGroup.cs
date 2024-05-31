using Core.Persistence.Repositories;

namespace Domain;

public class VariantGroup : Entity<string>
{
    public string? Name { get; set; }
    public ICollection<Product>? Products { get; set; }
}