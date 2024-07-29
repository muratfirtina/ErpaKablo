using Application.Features.Products.Commands.Create;
using Application.Features.Products.Dtos;
using Application.Features.Products.Rules;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.Persistence.Repositories.Operation;
using Domain;
using MediatR;

public class CreateMultipleProductsCommand : IRequest<List<CreatedProductResponse>>
{
    public List<CreateMultipleProductDto> Products { get; set; }

    public class CreateMultipleProductsCommandHandler : IRequestHandler<CreateMultipleProductsCommand, List<CreatedProductResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ProductBusinessRules _productBusinessRules;
        private readonly IStorageService _storageService;

        public CreateMultipleProductsCommandHandler(IProductRepository productRepository, IMapper mapper, ProductBusinessRules productBusinessRules, IStorageService storageService)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _productBusinessRules = productBusinessRules;
            _storageService = storageService;
        }
        public async Task<List<CreatedProductResponse>> Handle(CreateMultipleProductsCommand request, CancellationToken cancellationToken)
        {
            var responses = new List<CreatedProductResponse>();

            foreach (var productDto in request.Products)
            {
                var product = _mapper.Map<Product>(productDto);
                
                var normalizename = NameOperation.CharacterRegulatory(productDto.Name);
                var normalizesku = NameOperation.CharacterRegulatory(productDto.Sku);
                
                if (string.IsNullOrEmpty(productDto.VaryantGroupID))
                {
                    product.VaryantGroupID = $"{normalizename}-{normalizesku}";
                }

                product.ProductFeatureValues = new List<ProductFeatureValue>();
                if (productDto.FeatureValueIds != null)
                {
                    foreach (var featureValueId in productDto.FeatureValueIds)
                    {
                        product.ProductFeatureValues.Add(new ProductFeatureValue(product.Id, featureValueId));
                    }
                }

                await _productRepository.AddAsync(product);

                if (productDto.ProductImages != null && productDto.ProductImages.Any())
                {
                    var uploadedFiles = await _storageService.UploadAsync("products", product.Id, productDto.ProductImages);
                    foreach (var file in uploadedFiles)
                    {
                        var productImageFile = new ProductImageFile(file.fileName, file.category, file.path, file.storageType);
                        product.ProductImageFiles.Add(productImageFile);
                    }
                
                    await _productRepository.UpdateAsync(product);
                }

                var response = _mapper.Map<CreatedProductResponse>(product);
                responses.Add(response);
            }

            return responses;
        }

    }
}