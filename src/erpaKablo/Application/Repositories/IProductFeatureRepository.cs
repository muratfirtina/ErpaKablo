using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IProductFeatureRepository : IAsyncRepository<ProductFeature, int>, IRepository<ProductFeature, int>
{
    
}