using Core.Persistence.Repositories;

namespace Domain;

public class UIFilter : Entity<string>
{
    public string Name { get; set; }
    public virtual ICollection<CategoryFilter> CategoryFilters { get; set; }
    
    public UIFilter(string name) : base(name)
    {
        Name = name;
    }
}