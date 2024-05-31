using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class ProductVariantRepository : EfRepositoryBase<ProductVariant, string, ErpaKabloDbContext>, IProductVariantRepository
{
    public ProductVariantRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}