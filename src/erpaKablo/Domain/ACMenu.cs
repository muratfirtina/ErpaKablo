using Core.Persistence.Repositories;

namespace Domain;

public class ACMenu : Entity<int> //autohorization - controller menu
{
    public string Name { get; set; }
    public ICollection<Endpoint> Endpoints { get; set; }
}