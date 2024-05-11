using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class CategoryRepository : EfRepositoryBase<Category, int, ErpaKabloDbContext>, ICategoryRepository
{
    public CategoryRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}