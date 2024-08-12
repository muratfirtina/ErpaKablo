using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class CompletedOrderRepository:EfRepositoryBase<CompletedOrder,string,ErpaKabloDbContext>,ICompletedOrderRepository
{
    public CompletedOrderRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}