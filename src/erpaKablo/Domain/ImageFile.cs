using System.ComponentModel.DataAnnotations.Schema;
using Core.Persistence.Repositories;

namespace Domain;

public class ImageFile: Entity<string>
{
    public string? Name { get; set; }
    public string EntityType { get; set; }
    public string Path { get; set; }
    [NotMapped]
    public string Url { get; set; }
    public string Storage { get; set; }
    
    public ImageFile(string? name) : base(name)
    {
        Name = name;
    }
    
}