using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class ProductLikeRepository : EfRepositoryBase<ProductLike, string, ErpaKabloDbContext>, IProductLikeRepository
{
    public ProductLikeRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}