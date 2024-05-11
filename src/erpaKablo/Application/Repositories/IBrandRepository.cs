using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IBrandRepository : IAsyncRepository<Brand, int>, IRepository<Brand, int>
{
    
}