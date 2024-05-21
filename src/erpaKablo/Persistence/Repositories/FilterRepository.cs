using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class FilterRepository : EfRepositoryBase<Filter,string, ErpaKabloDbContext>, IFilterRepository
{
    public FilterRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}