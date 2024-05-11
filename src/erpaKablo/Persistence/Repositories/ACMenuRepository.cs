using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class ACMenuRepository : EfRepositoryBase<ACMenu, int, ErpaKabloDbContext>, IACMenuRepository
{
    public ACMenuRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}