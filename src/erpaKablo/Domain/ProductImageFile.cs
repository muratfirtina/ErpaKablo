namespace Domain;

public class ProductImageFile:ImageFile
{
    public bool Showcase { get; set; }
    public string? Alt { get; set; }
    public ICollection<Product>? Products { get; set; }

    public ProductImageFile(string name) : base(name)
    {
        Name = name;
    }
}