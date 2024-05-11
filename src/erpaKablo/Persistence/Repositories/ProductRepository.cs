using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class ProductRepository : EfRepositoryBase<Product, int, ErpaKabloDbContext>, IProductRepository
{
    public ProductRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}