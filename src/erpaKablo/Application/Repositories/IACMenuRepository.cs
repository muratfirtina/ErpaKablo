using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IACMenuRepository : IAsyncRepository<ACMenu, int>, IRepository<ACMenu, int>
{
    
}