using Application.Features.ProductFeatures.Rules;
using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductFeatures.Commands.Update;

public class UpdateProductFeatureCommand : IRequest<UpdatedProductFeatureResponse>
{
    public string Id { get; set; }
    public string Name { get; set; }

    public class UpdateProductFeatureCommandHandler : IRequestHandler<UpdateProductFeatureCommand, UpdatedProductFeatureResponse>
    {
        private readonly IProductFeatureRepository _productFeatureRepository;
        private readonly ProductFeatureBusinessRules _productFeatureBusinessRules;
        private readonly IMapper _mapper;

        public UpdateProductFeatureCommandHandler(IProductFeatureRepository productFeatureRepository, IMapper mapper, ProductFeatureBusinessRules productFeatureBusinessRules)
        {
            _productFeatureRepository = productFeatureRepository;
            _mapper = mapper;
            _productFeatureBusinessRules = productFeatureBusinessRules;
        }

        public async Task<UpdatedProductFeatureResponse> Handle(UpdateProductFeatureCommand request,
            CancellationToken cancellationToken)
        {
            ProductFeature? productFeature = await _productFeatureRepository.GetAsync(p => p.Id == request.Id,
                cancellationToken: cancellationToken);
            await _productFeatureBusinessRules.ProductFeatureShouldExistWhenSelected(productFeature);
            if (productFeature != null)
            {
                productFeature = _mapper.Map(request, productFeature);
                await _productFeatureRepository.UpdateAsync(productFeature);
                UpdatedProductFeatureResponse response = _mapper.Map<UpdatedProductFeatureResponse>(productFeature);
                return response;
            }
            throw new Exception("ProductFeature not found");
        }
    }
}