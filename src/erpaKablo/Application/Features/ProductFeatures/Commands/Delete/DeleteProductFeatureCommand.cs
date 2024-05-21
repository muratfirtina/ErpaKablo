using Application.Features.ProductFeatures.Rules;
using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;

namespace Application.Features.ProductFeatures.Commands.Delete;

public class DeleteProductFeatureCommand : IRequest<DeletedProductFeatureResponse>
{
    public string Id { get; set; }
    
    public class DeleteProductFeatureCommandHandler : IRequestHandler<DeleteProductFeatureCommand, DeletedProductFeatureResponse>
    {
        private readonly IProductFeatureRepository _productFeatureRepository;
        private readonly ProductFeatureBusinessRules _productFeatureBusinessRules;
        private readonly IMapper _mapper;

        public DeleteProductFeatureCommandHandler(IProductFeatureRepository productFeatureRepository, IMapper mapper, ProductFeatureBusinessRules productFeatureBusinessRules)
        {
            _productFeatureRepository = productFeatureRepository;
            _mapper = mapper;
            _productFeatureBusinessRules = productFeatureBusinessRules;
        }

        public async Task<DeletedProductFeatureResponse> Handle(DeleteProductFeatureCommand request, CancellationToken cancellationToken)
        {
            ProductFeature? productFeature = await _productFeatureRepository.GetAsync(p=>p.Id==request.Id,cancellationToken: cancellationToken);
            await _productFeatureBusinessRules.ProductFeatureShouldExistWhenSelected(productFeature);
            await _productFeatureRepository.DeleteAsync(productFeature!);
            DeletedProductFeatureResponse response = _mapper.Map<DeletedProductFeatureResponse>(productFeature);
            return response;
        }
    }
}