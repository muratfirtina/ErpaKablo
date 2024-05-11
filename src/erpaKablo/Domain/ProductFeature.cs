using Core.Persistence.Repositories;

namespace Domain;

public class ProductFeature : Entity<int>
{
    public string? Name { get; set; }
    public int ProductId { get; set; }
    public int FilterId { get; set; }
    public Product? Product { get; set; }
    public CategoryFilter? Filter { get; set; }
}