using System.ComponentModel.DataAnnotations.Schema;
using Core.Persistence.Repositories;

namespace Domain;

public class ImageFile: Entity<int>
{
    public string FileName { get; set; }
    public string Category { get; set; }
    public string Path { get; set; }
    [NotMapped]
    public string Url { get; set; }
    public string Storage { get; set; }
}