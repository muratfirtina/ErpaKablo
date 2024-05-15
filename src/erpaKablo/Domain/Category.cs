using Core.Persistence.Repositories;

namespace Domain;

public class Category:Entity<string>
{
    public string? Name { get; set; }
    public string? ParentCategoryId { get; set; }  // Üst kategori null olabilir (ana kategori için)
    public Category? ParentCategory { get; set; }
    public ICollection<Category>? SubCategories { get; set; }
    public ICollection<Product>? Products { get; set; }
    public ICollection<CategoryFilter>? CategoryFilters { get; set; }
    
    public Category(string name) : base(name)
    {
        Name = name;
    }
    
}