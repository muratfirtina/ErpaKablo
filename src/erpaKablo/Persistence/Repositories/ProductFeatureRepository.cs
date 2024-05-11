using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class ProductFeatureRepository : EfRepositoryBase<ProductFeature, int, ErpaKabloDbContext>, IProductFeatureRepository
{
    public ProductFeatureRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}