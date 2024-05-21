using Application.Features.Features.Rules;
using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;

namespace Application.Features.Features.Commands.Delete;

public class DeleteFeatureCommand : IRequest<DeletedFeatureResponse>
{
    public string Id { get; set; }
    
    public class DeleteFeatureCommandHandler : IRequestHandler<DeleteFeatureCommand, DeletedFeatureResponse>
    {
        private readonly IFeatureRepository _featureRepository;
        private readonly FeatureBusinessRules _featureBusinessRules;
        private readonly IMapper _mapper;

        public DeleteFeatureCommandHandler(IFeatureRepository featureRepository, IMapper mapper, FeatureBusinessRules featureBusinessRules)
        {
            _featureRepository = featureRepository;
            _mapper = mapper;
            _featureBusinessRules = featureBusinessRules;
        }

        public async Task<DeletedFeatureResponse> Handle(DeleteFeatureCommand request, CancellationToken cancellationToken)
        {
            Feature? feature = await _featureRepository.GetAsync(p=>p.Id==request.Id,cancellationToken: cancellationToken);
            await _featureBusinessRules.FeatureShouldExistWhenSelected(feature);
            await _featureRepository.DeleteAsync(feature!);
            DeletedFeatureResponse response = _mapper.Map<DeletedFeatureResponse>(feature);
            return response;
        }
    }
}