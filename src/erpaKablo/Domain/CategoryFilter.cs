using Core.Persistence.Repositories;

namespace Domain;

public class CategoryFilter:Entity<string>
{
    public string Name { get; set; }
    public string? CategoryId { get; set; }
    public virtual Category Category { get; set; }
    public string? FilterId { get; set; }
    public virtual Filter Filter { get; set; }
}