using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IProductVariantRepository : IAsyncRepository<ProductVariant, string>, IRepository<ProductVariant, string>
{
    Task<int> GetProductCountByFeatureValueId(string featureValueId);
}