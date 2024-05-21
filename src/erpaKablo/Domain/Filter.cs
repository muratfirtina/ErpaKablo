using Core.Persistence.Repositories;

namespace Domain;

public class Filter : Entity<string>
{
    public string? Name { get; set; }
    public virtual ICollection<CategoryFilter> CategoryFilters { get; set; }
    
    public Filter(string? name) : base(name)
    {
        Name = name;
    }
}