using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class CartRepository:EfRepositoryBase<Cart,string,ErpaKabloDbContext>,ICartRepository
{
    public CartRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}