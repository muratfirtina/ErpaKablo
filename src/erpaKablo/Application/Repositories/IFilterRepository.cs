using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IFilterRepository : IAsyncRepository<UIFilter, string>, IRepository<UIFilter, string>
{
    
}