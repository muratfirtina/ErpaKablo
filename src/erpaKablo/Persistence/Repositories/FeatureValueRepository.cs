using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class FeatureValueRepository : EfRepositoryBase<FeatureValue, string, ErpaKabloDbContext>, IFeatureValueRepository
{
    public FeatureValueRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}