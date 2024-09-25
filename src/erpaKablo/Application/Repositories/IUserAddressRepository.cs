using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IUserAddressRepository : IAsyncRepository<UserAddress, string>, IRepository<UserAddress, string>
{
    
}