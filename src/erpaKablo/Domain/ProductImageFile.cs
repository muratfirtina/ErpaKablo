namespace Domain;

public class ProductImageFile:ImageFile
{
    public bool Showcase { get; set; } = false;
    public string? Alt { get; set; }
    public string? ProductId { get; set; }
    public Product? Product { get; set; }
    

    public ProductImageFile(string? name) : base(name)
    {
    }

    public ProductImageFile() : base(null)
    {
    }
}