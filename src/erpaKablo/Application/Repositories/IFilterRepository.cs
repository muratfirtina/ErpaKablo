using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IFilterRepository : IAsyncRepository<UIFilter, int>, IRepository<UIFilter, int>
{
    
}