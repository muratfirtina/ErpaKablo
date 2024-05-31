using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IVariantGroupRepository : IAsyncRepository<VariantGroup, string>, IRepository<VariantGroup, string>
{
    
}