namespace Domain;

public class ProductImageFile:ImageFile
{
    public bool Showcase { get; set; } = false;
    public string? Alt { get; set; }
    public ICollection<Product> Products { get; set; }
    

    public ProductImageFile(string? name) : base(name)
    {
        Products = new List<Product>();
    }

    public ProductImageFile() : base(null)
    {
        
    }
}