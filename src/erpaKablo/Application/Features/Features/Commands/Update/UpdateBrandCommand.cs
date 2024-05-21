using Application.Features.Features.Rules;
using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Features.Commands.Update;

public class UpdateFeatureCommand : IRequest<UpdatedFeatureResponse>
{
    public string Id { get; set; }
    public string Name { get; set; }

    public class UpdateFeatureCommandHandler : IRequestHandler<UpdateFeatureCommand, UpdatedFeatureResponse>
    {
        private readonly IFeatureRepository _featureRepository;
        private readonly FeatureBusinessRules _featureBusinessRules;
        private readonly IMapper _mapper;

        public UpdateFeatureCommandHandler(IFeatureRepository featureRepository, IMapper mapper, FeatureBusinessRules featureBusinessRules)
        {
            _featureRepository = featureRepository;
            _mapper = mapper;
            _featureBusinessRules = featureBusinessRules;
        }

        public async Task<UpdatedFeatureResponse> Handle(UpdateFeatureCommand request,
            CancellationToken cancellationToken)
        {
            Feature? feature = await _featureRepository.GetAsync(p => p.Id == request.Id,
                cancellationToken: cancellationToken);
            await _featureBusinessRules.FeatureShouldExistWhenSelected(feature);
            if (feature != null)
            {
                feature = _mapper.Map(request, feature);
                await _featureRepository.UpdateAsync(feature);
                UpdatedFeatureResponse response = _mapper.Map<UpdatedFeatureResponse>(feature);
                return response;
            }
            throw new Exception("Feature not found");
        }
    }
}