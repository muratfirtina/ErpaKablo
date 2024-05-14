using Core.Persistence.Repositories;

namespace Domain;

public class UIFilter : Entity<int>
{
    public string Name { get; set; }
    public virtual ICollection<CategoryFilter> CategoryFilters { get; set; }
}