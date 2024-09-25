using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class UserAddressRepository : EfRepositoryBase<UserAddress, string, ErpaKabloDbContext>, IUserAddressRepository
{
    public UserAddressRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}