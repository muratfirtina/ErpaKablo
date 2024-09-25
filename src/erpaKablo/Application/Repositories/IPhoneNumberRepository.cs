using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IPhoneNumberRepository: IAsyncRepository<PhoneNumber, string> , IRepository<PhoneNumber, string>
{
    
}