using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class FeatureRepository : EfRepositoryBase<Feature, int, ErpaKabloDbContext>, IFeatureRepository
{
    public FeatureRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}