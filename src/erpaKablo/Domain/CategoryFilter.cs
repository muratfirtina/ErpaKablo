using Core.Persistence.Repositories;

namespace Domain;

public class CategoryFilter:Entity<int>
{
    public int CategoryId { get; set; }
    public virtual Category Category { get; set; }
    public int FilterId { get; set; }
    public virtual UIFilter Filter { get; set; }
}