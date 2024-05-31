using Application.Features.Products.Dtos;
using Application.Features.Products.Rules;
using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.Update;

public class UpdateProductCommand : IRequest<UpdatedProductResponse>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? CategoryId { get; set; }
    public string? BrandId { get; set; }
    public string? Description { get; set; }
    public ICollection<ProductFeatureDto> ProductFeatures { get; set; }

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, UpdatedProductResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly IFeatureRepository _featureRepository;
        private readonly ProductBusinessRules _productBusinessRules;
        private readonly IMapper _mapper;

        public UpdateProductCommandHandler(IProductRepository productRepository, IMapper mapper, ProductBusinessRules productBusinessRules, IFeatureRepository featureRepository)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _productBusinessRules = productBusinessRules;
            _featureRepository = featureRepository;
        }

        public async Task<UpdatedProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
{
    // Ürün var mı kontrolü
    Product? product = await _productRepository.GetAsync(
        p => p.Id == request.Id,
        include: p => p.Include(e => e.Category)
            .Include(e => e.Brand)!,
        cancellationToken: cancellationToken);

    await _productBusinessRules.ProductShouldExistWhenSelected(product);

    if (product != null)
    {
        product.CategoryId = request.CategoryId;
        product.BrandId = request.BrandId;
        product.Description = request.Description;
        product.Name = request.Name;

        // Mevcut özellikleri temizleme

        foreach (var productFeatureDto in request.ProductFeatures)
        {
            foreach (var featureDetail in productFeatureDto.FeatureValues)
            {
                // Mevcut özelliği veritabanından bulma
                var existingFeature = await _featureRepository.Query()
                    .Include(f => f.FeatureValues)
                    .FirstOrDefaultAsync(f => f.Id == featureDetail.Id);

                if (existingFeature != null)
                {
                    var featureValues = new List<FeatureValue>();
                    foreach (var featureValueDto in productFeatureDto.FeatureValues)
                    {
                        var existingFeatureValue = existingFeature.FeatureValues
                            .FirstOrDefault(fv => fv.Id == featureValueDto.Id);
                        if (existingFeatureValue != null)
                        {
                            featureValues.Add(existingFeatureValue);
                        }
                    }

                    // Özelliği ürüne ekleme
                    
                }
            }
        }

        await _productRepository.UpdateAsync(product);
        UpdatedProductResponse response = _mapper.Map<UpdatedProductResponse>(product);
        return response;
    }

    throw new Exception("Product not found");
}
    }
}