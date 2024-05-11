using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class EndpointRepository : EfRepositoryBase<Endpoint, int, ErpaKabloDbContext>, IEndpointRepository
{
    public EndpointRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}