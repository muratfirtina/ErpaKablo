using Application.Features.FeatureValues.Commands.Create;
using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;

namespace Application.Features.FeatureValues.Commands.Create;

public class CreateFeatureValueCommand : IRequest<CreatedFeatureValueResponse>
{
    public string Value { get; set; }
    public string FeatureId { get; set; }
    
    public class CreateFeatureCommandHandler : IRequestHandler<CreateFeatureValueCommand, CreatedFeatureValueResponse>
    {
        private readonly IMapper _mapper;
        private readonly IFeatureValueRepository _featureValueRepository;

        public CreateFeatureCommandHandler(IMapper mapper, IFeatureValueRepository featureValueRepository)
        {
            _mapper = mapper;
            _featureValueRepository = featureValueRepository;
        }

        public async Task<CreatedFeatureValueResponse> Handle(CreateFeatureValueCommand request, CancellationToken cancellationToken)
        {
            var featureValue = _mapper.Map<FeatureValue>(request);
            await _featureValueRepository.AddAsync(featureValue);
            
            CreatedFeatureValueResponse response = _mapper.Map<CreatedFeatureValueResponse>(featureValue);
            return response;
        }
    }
    
}