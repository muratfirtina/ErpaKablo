using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class CategoryFilterRepository : EfRepositoryBase<CategoryFilter, int, ErpaKabloDbContext>, ICategoryFilterRepository
{
    public CategoryFilterRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}