using System.ComponentModel.DataAnnotations.Schema;

namespace Domain;

public class ProductImageFile:ImageFile
{
    public bool Showcase { get; set; } = false;
    public string? Alt { get; set; }
    public ICollection<Product> Products { get; set; }
    [NotMapped]
    public string Url { get; set; }
    

    public ProductImageFile(string? name) : base(name)
    {
        Name = name;
    }

    public ProductImageFile(string? name, string? category) : base(name)
    {
        Name = name;
        Category = category;
    }
    
    public ProductImageFile(string? name, string? category, string? path) : base(name)
    {
        Name = name;
        Category = category;
        Path = path;
    }
    
    public ProductImageFile(string? name, string? category, string? path, string? storage) : base(name)
    {
        Name = name;
        Category = category;
        Path = path;
        Storage = storage;
        
    }

    public ProductImageFile() : base(null)
    {
    }
    
}