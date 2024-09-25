using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class ProductViewRepository : EfRepositoryBase<ProductView, string, ErpaKabloDbContext>, IProductViewRepository
{
    public ProductViewRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}