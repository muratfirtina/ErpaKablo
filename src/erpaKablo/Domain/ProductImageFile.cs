namespace Domain;

public class ProductImageFile:ImageFile
{
    public bool Showcase { get; set; }
    public ICollection<Product>? Products { get; set; }
}