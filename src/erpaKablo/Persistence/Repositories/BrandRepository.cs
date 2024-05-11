using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class BrandRepository : EfRepositoryBase<Brand, int, ErpaKabloDbContext>, IBrandRepository
{
    public BrandRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}