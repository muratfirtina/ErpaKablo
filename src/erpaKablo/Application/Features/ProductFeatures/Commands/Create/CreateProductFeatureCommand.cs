using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;

namespace Application.Features.ProductFeatures.Commands.Create;

public class CreateProductFeatureCommand : IRequest<CreatedProductFeatureResponse>
{
    public string Name { get; set; }
    
    public class CreateProductFeatureCommandHandler : IRequestHandler<CreateProductFeatureCommand, CreatedProductFeatureResponse>
    {
        private readonly IMapper _mapper;
        private readonly IProductFeatureRepository _productFeatureRepository;

        public CreateProductFeatureCommandHandler(IMapper mapper, IProductFeatureRepository productFeatureRepository)
        {
            _mapper = mapper;
            _productFeatureRepository = productFeatureRepository;
        }

        public async Task<CreatedProductFeatureResponse> Handle(CreateProductFeatureCommand request, CancellationToken cancellationToken)
        {
            var productFeature = _mapper.Map<ProductFeature>(request);
            await _productFeatureRepository.AddAsync(productFeature);
            
            CreatedProductFeatureResponse response = _mapper.Map<CreatedProductFeatureResponse>(productFeature);
            return response;
        }
    }
    
}