using Application.Features.Products.Commands.Create;
using Application.Features.Products.Dtos;
using Application.Features.Products.Rules;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.Persistence.Repositories.Operation;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class CreateMultipleProductsCommand : IRequest<List<CreatedProductResponse>>
{
    public List<CreateMultipleProductDto> Products { get; set; }

    public class
        CreateMultipleProductsCommandHandler : IRequestHandler<CreateMultipleProductsCommand,
        List<CreatedProductResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;
        private readonly ProductBusinessRules _productBusinessRules;
        private readonly IStorageService _storageService;
        private readonly ICategoryRepository _categoryRepository;

        public CreateMultipleProductsCommandHandler(IProductRepository productRepository, IMapper mapper,
            ProductBusinessRules productBusinessRules, IStorageService storageService,
            ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _productBusinessRules = productBusinessRules;
            _storageService = storageService;
            _categoryRepository = categoryRepository;
        }

        public async Task<List<CreatedProductResponse>> Handle(CreateMultipleProductsCommand request,
            CancellationToken cancellationToken)
        {
            var responses = new List<CreatedProductResponse>();

            foreach (var productDto in request.Products)
            {
                // Kategori kontrolü
                var category = await _categoryRepository.GetAsync(c => c.Id == productDto.CategoryId);
                if (category == null)
                {
                    throw new Exception($"Category with ID {productDto.CategoryId} not found.");
                }

                var product = _mapper.Map<Product>(productDto);

                var normalizename = NameOperation.CharacterRegulatory(productDto.Name);
                var normalizesku = NameOperation.CharacterRegulatory(productDto.Sku);

                if (string.IsNullOrEmpty(productDto.VaryantGroupID))
                {
                    product.VaryantGroupID = $"{normalizename}-{normalizesku}";
                }

                product.CategoryId = productDto.CategoryId; // Kategori ID'sini atıyoruz

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
                    var uploadedFiles =
                        await _storageService.UploadAsync("products", product.Id, productDto.ProductImages);
                    for (int i = 0; i < uploadedFiles.Count; i++)
                    {
                        var file = uploadedFiles[i];
                        var productImageFile =
                            new ProductImageFile(file.fileName, file.entityType, file.path, file.storageType)
                            {
                                Showcase = i == productDto.ShowcaseImageIndex,
                                Format = file.format
                            };
                        product.ProductImageFiles.Add(productImageFile);
                    }

                    await _productBusinessRules.EnsureOnlyOneShowcaseImage(product.ProductImageFiles);
                    await _productRepository.UpdateAsync(product);
                }

                var response = _mapper.Map<CreatedProductResponse>(product);
                responses.Add(response);
            }

            return responses;
        }
    }
}