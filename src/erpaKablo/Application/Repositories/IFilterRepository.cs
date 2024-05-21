using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IFilterRepository : IAsyncRepository<Filter, string>, IRepository<Filter, string>
{
    
}