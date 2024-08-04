using Application.Features.ProductImageFiles.Dtos;
using Application.Features.Products.Dtos;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using AutoMapper.Internal;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetById;

public class GetByIdProductQuery : IRequest<GetByIdProductResponse>
{
    public string Id { get; set; }
    
    public class GetByIdProductQueryHandler : IRequestHandler<GetByIdProductQuery, GetByIdProductResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;

        public GetByIdProductQueryHandler(IProductRepository productRepository, IMapper mapper, IStorageService storageService)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<GetByIdProductResponse> Handle(GetByIdProductQuery request, CancellationToken cancellationToken)
{
    Product product = await _productRepository.GetAsync(
        predicate: p => p.Id == request.Id,
        cancellationToken: cancellationToken,
        include: x => x.Include(x => x.Category)
            .Include(x => x.Brand)
            .Include(x => x.ProductFeatureValues).ThenInclude(x => x.FeatureValue).ThenInclude(x => x.Feature)
            .Include(x => x.ProductImageFiles));

    var relatedProducts = await _productRepository.GetListAsync(
        predicate: p => p.VaryantGroupID == product.VaryantGroupID && p.Id != product.Id,
        include: x => x.Include(x => x.Category).Include(x => x.Brand)
            .Include(x => x.ProductFeatureValues).ThenInclude(x => x.FeatureValue).ThenInclude(x => x.Feature)
            .Include(x => x.ProductImageFiles.Where(pif => pif.Showcase == true)),
        cancellationToken: cancellationToken);

    GetByIdProductResponse response = _mapper.Map<GetByIdProductResponse>(product);
    response.RelatedProducts = _mapper.Map<List<RelatedProductDto>>(relatedProducts.Items);

    // URL'leri güncelle
    var baseUrl = _storageService.GetStorageUrl();
    foreach (var imageFile in response.ProductImageFiles)
    {
        imageFile.Url = $"{baseUrl}{imageFile.Category}/{imageFile.Path}/{imageFile.FileName}";
    }
    
    foreach (var relatedProduct in response.RelatedProducts)
    {
        if (relatedProduct.ShowcaseImage != null)
        {
            relatedProduct.ShowcaseImage.Url = $"{baseUrl}{relatedProduct.ShowcaseImage.Category}/{relatedProduct.ShowcaseImage.Path}/{relatedProduct.ShowcaseImage.FileName}";
        }
    }

    // Mevcut özellikleri hesapla
    response.AvailableFeatures = new Dictionary<string, List<string>>();
    foreach (var relatedProduct in response.RelatedProducts)
    {
        foreach (var feature in relatedProduct.ProductFeatureValues)
        {
            if (!response.AvailableFeatures.ContainsKey(feature.FeatureName))
            {
                response.AvailableFeatures[feature.FeatureName] = new List<string>();
            }
            if (!response.AvailableFeatures[feature.FeatureName].Contains(feature.FeatureValueName))
            {
                response.AvailableFeatures[feature.FeatureName].Add(feature.FeatureValueName);
            }
        }
    }

    return response;
}
    }
}