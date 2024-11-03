using Application.Features.PhoneNumbers.Dtos;
using Application.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.PhoneNumbers.Commands.Create;

public class CreatePhoneNumberCommand : IRequest<CreatedPhoneNumberCommandResponse>
{
    public string Name { get; set; }
    public string Number { get; set; }
    public bool IsDefault { get; set; }

    public class CreatePhoneNumberCommandHandler : IRequestHandler<CreatePhoneNumberCommand, CreatedPhoneNumberCommandResponse>
    {
        private readonly IPhoneNumberRepository _phoneNumberRepository;
        private readonly IMapper _mapper;

        public CreatePhoneNumberCommandHandler(IPhoneNumberRepository phoneNumberRepository, IMapper mapper)
        {
            _phoneNumberRepository = phoneNumberRepository;
            _mapper = mapper;
        }

        public async Task<CreatedPhoneNumberCommandResponse> Handle(CreatePhoneNumberCommand request, CancellationToken cancellationToken)
        {
            var phoneDto = _mapper.Map<CreatePhoneNumberDto>(request);
            var phone = await _phoneNumberRepository.AddPhoneAsync(phoneDto);
            return _mapper.Map<CreatedPhoneNumberCommandResponse>(phone);
        }
    }
}