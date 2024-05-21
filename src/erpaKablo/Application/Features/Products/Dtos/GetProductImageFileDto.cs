namespace Application.Features.Products.Dtos;

public class GetProductImageFileDto
{
    public string? Category { get; set; }
    public string? Path { get; set; }
    public string? Name { get; set; }
    public string? Url { get; set; }
    public string Id { get; set; }
    public string? Storage { get; set; }
    public string? Alt { get; set; }
    public bool Showcase { get; set; }
    
}