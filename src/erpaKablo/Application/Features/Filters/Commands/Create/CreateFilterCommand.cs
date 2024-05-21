using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;

namespace Application.Features.Filters.Commands.Create;

public class CreateFilterCommand : IRequest<CreatedFilterResponse>
{
    public string Name { get; set; }
    
    public class CreateFilterCommandHandler : IRequestHandler<CreateFilterCommand, CreatedFilterResponse>
    {
        private readonly IMapper _mapper;
        private readonly IFilterRepository _filterRepository;

        public CreateFilterCommandHandler(IMapper mapper, IFilterRepository filterRepository)
        {
            _mapper = mapper;
            _filterRepository = filterRepository;
        }

        public async Task<CreatedFilterResponse> Handle(CreateFilterCommand request, CancellationToken cancellationToken)
        {
            var filter = _mapper.Map<Filter>(request);
            await _filterRepository.AddAsync(filter);
            
            CreatedFilterResponse response = _mapper.Map<CreatedFilterResponse>(filter);
            return response;
        }
    }
    
}