using Core.Persistence.Repositories;

namespace Domain;

public class CategoryFilter:Entity<int>
{
    public string? Name { get; set; }
    public ICollection<Category>? Categories { get; set; }
}