using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IOrderRepository: IAsyncRepository<Order, string>, IRepository<Order, string>
{
    
}