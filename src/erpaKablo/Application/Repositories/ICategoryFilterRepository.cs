using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface ICategoryFilterRepository : IAsyncRepository<CategoryFilter, string>, IRepository<CategoryFilter, string>
{
    
}