using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class OrderRepositories:EfRepositoryBase<Order,string,ErpaKabloDbContext>,IOrderRepository
{
    public OrderRepositories(ErpaKabloDbContext context) : base(context)
    {
    }
}