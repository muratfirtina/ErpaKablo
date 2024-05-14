using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IFeatureRepository : IAsyncRepository<Feature, int>, IRepository<Feature, int>
{
    
}