using Core.Persistence.Repositories;

namespace Domain;

public class Category:Entity<int>
{
    public string? Name { get; set; }
    public int? ParentCategoryId { get; set; }  // Üst kategori null olabilir (ana kategori için)
    public Category? ParentCategory { get; set; }
    public ICollection<Category>? SubCategories { get; set; }
    public ICollection<Product>? Products { get; set; }
    public ICollection<CategoryFilter>? Filters { get; set; }
    
}