using Application.Repositories;
using MediatR;

namespace Application.Features.UserAddresses.Commands.Delete;

public class DeleteUserAddressCommand:IRequest<bool>
{
    public string Id { get; set; }
    


    public class DelereUserAddressCommandHandler : IRequestHandler<DeleteUserAddressCommand, bool>
    {
        private readonly IUserAddressRepository _addressRepository;

        public DelereUserAddressCommandHandler(IUserAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        public Task<bool> Handle(DeleteUserAddressCommand request, CancellationToken cancellationToken)
        {
            return _addressRepository.DeleteAddressAsync(request.Id);
        }
    }

}