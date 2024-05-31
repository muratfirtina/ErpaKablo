using Application.Features.Products.Dtos;
using Application.Features.Products.Rules;
using Application.Repositories;
using AutoMapper;
using Core.CrossCuttingConcerns.Exceptions;
using Domain;
using MediatR;

namespace Application.Features.Products.Commands.Create;

public class CreateProductCommand : IRequest<CreatedProductResponse>
{
    public CreateProductCommand(CreateProductDto createProductDto)
    {
        CreateProductDto = createProductDto;
    }

    public CreateProductDto CreateProductDto { get; set; }
    
    public class CreateProductCommandHandler : IRequestHandler<CreateProductCommand, CreatedProductResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ProductBusinessRules _productBusinessRules;

        public CreateProductCommandHandler(IProductRepository productRepository, IMapper mapper, ProductBusinessRules productBusinessRules)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _productBusinessRules = productBusinessRules;
        }

        public async Task<CreatedProductResponse> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            
            var product = _mapper.Map<Product>(request);

            // Varyantları ve varyant özelliklerini ekleme
            if (request.CreateProductDto.Variants != null && request.CreateProductDto.Variants.Any())
            {
                foreach (var variantDto in request.CreateProductDto.Variants)
                {
                    var variant = new ProductVariant
                    {
                        //ProductVariant ın id si ürün id si ve featurevalue.value birleşimi olacak
                        Id = $"{product.Id}-{string.Join("-", variantDto.Features.Select(f => f.FeatureValueId).First())}",
                        ProductId = product.Id,
                        Price = variantDto.Price,
                        Stock = variantDto.Stock,
                        VariantFeatureValues = new List<VariantFeatureValue>()
                    };

                    if (variantDto.Features != null && variantDto.Features.Any())
                    {
                        foreach (var featureDto in variantDto.Features)
                        {
                            var variantFeature = new VariantFeatureValue
                            {
                                
                                ProductVariantId = variant.Id,
                                FeatureId = featureDto.FeatureId,
                                FeatureValueId = featureDto.FeatureValueId
                            };
                            variant.VariantFeatureValues.Add(variantFeature);
                        }
                    }
                    product.ProductVariants.Add(variant);
                }
            }

            await _productRepository.AddAsync(product);
            
            CreatedProductResponse response = _mapper.Map<CreatedProductResponse>(product);
            return response;
        }
    }
}

    
