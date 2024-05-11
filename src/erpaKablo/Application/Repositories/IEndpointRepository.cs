using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IEndpointRepository : IAsyncRepository<Endpoint, int>, IRepository<Endpoint, int>
{
    
}