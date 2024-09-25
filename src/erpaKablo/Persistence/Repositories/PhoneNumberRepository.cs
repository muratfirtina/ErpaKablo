using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class PhoneNumberRepository : EfRepositoryBase<PhoneNumber, string, ErpaKabloDbContext>, IPhoneNumberRepository
{
    public PhoneNumberRepository(ErpaKabloDbContext dbContext) : base(dbContext)
    {
    }
}