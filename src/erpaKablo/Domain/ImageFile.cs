using Core.Persistence.Repositories;

namespace Domain;

public class ImageFile: Entity<int>
{
    public string? FileName { get; set; }
    public string? Path { get; set; }
    public string? Storage { get; set; }
}