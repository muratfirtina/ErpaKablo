using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IProductRepository : IAsyncRepository<Product, int>, IRepository<Product, int>
{
    
}