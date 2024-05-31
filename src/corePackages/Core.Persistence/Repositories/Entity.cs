using Core.Persistence.Repositories.Operation;

namespace Core.Persistence.Repositories;

public abstract class Entity<TId> : IEntity<TId>, IEntityTimestamps
{
    public TId Id { get; set; }
    public DateTime CreatedDate { get; set; }
    public DateTime? UpdatedDate { get; set; }
    public DateTime? DeletedDate { get; set; }

    public Entity()
    {
        Id = default!;
    }

    protected Entity(string? name)
    {
        Id = (TId)(object)IdGenerator.GenerateId(name);
    }
    
    protected Entity(string? name, string? sku)
    {
        Id = (TId)(object)IdGenerator.GenerateIdwithSku(name, sku);
    }
}