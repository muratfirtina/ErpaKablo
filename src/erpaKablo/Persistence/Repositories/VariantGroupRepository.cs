using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class VariantGroupRepository : EfRepositoryBase<VariantGroup, string, ErpaKabloDbContext>, IVariantGroupRepository
{
    public VariantGroupRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}