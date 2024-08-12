using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class CartItemRepository:EfRepositoryBase<CartItem,string,ErpaKabloDbContext>,ICartItemRepository
{
    public CartItemRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}