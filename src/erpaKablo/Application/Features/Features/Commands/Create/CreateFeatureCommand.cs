using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;

namespace Application.Features.Features.Commands.Create;

public class CreateFeatureCommand : IRequest<CreatedFeatureResponse>
{
    public string Name { get; set; }
    
    public class CreateFeatureCommandHandler : IRequestHandler<CreateFeatureCommand, CreatedFeatureResponse>
    {
        private readonly IMapper _mapper;
        private readonly IFeatureRepository _featureRepository;

        public CreateFeatureCommandHandler(IMapper mapper, IFeatureRepository featureRepository)
        {
            _mapper = mapper;
            _featureRepository = featureRepository;
        }

        public async Task<CreatedFeatureResponse> Handle(CreateFeatureCommand request, CancellationToken cancellationToken)
        {
            var feature = _mapper.Map<Feature>(request);
            await _featureRepository.AddAsync(feature);
            
            CreatedFeatureResponse response = _mapper.Map<CreatedFeatureResponse>(feature);
            return response;
        }
    }
    
}