namespace Application.Features.Products.Dtos;

public class GetProductImageFileDto
{
    public string? Category { get; set; }
    public string? Path { get; set; }
    public string? FileName { get; set; }
    public string? Url { get; set; }
    public int Id { get; set; }
    public string? Storage { get; set; }
    
    public bool Showcase { get; set; }
}