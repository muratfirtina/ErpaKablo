using Application.Repositories;
using MediatR;

namespace Application.Features.PhoneNumbers.Commands.Delete;

public class DeletePhoneNumberCommand : IRequest<DeletedPhoneNumberCommandResponse>
{
    public string Id { get; set; }

    public class DeletePhoneNumberCommandHandler : IRequestHandler<DeletePhoneNumberCommand, DeletedPhoneNumberCommandResponse>
    {
        private readonly IPhoneNumberRepository _phoneNumberRepository;

        public DeletePhoneNumberCommandHandler(IPhoneNumberRepository phoneNumberRepository)
        {
            _phoneNumberRepository = phoneNumberRepository;
        }

        public async Task<DeletedPhoneNumberCommandResponse> Handle(DeletePhoneNumberCommand request, CancellationToken cancellationToken)
        {
            var result = await _phoneNumberRepository.DeletePhoneAsync(request.Id);
            return new DeletedPhoneNumberCommandResponse { Success = result };
        }
    }
}