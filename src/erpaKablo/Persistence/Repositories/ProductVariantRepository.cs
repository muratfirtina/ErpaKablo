using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class ProductVariantRepository : EfRepositoryBase<ProductVariant, string, ErpaKabloDbContext>, IProductVariantRepository
{
    public ProductVariantRepository(ErpaKabloDbContext context) : base(context)
    {
    }

    public async Task<int> GetProductCountByFeatureValueId(string featureValueId)
    {
        return await Context.ProductVariants
            .Where(pv => pv.VariantFeatureValues.Any(vfv => vfv.FeatureValueId == featureValueId))
            .CountAsync();
    }
}