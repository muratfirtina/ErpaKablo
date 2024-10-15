using Application.Features.Products.Dtos;
using Application.Features.Products.Rules;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Commands.Update;

public class UpdateProductCommand : IRequest<UpdatedProductResponse>
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string CategoryId { get; set; }
    public string BrandId { get; set; }
    public string VaryantGroupID { get; set; }
    public string Sku { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public int Tax { get; set; }
    public List<ProductFeatureDto> ProductFeatures { get; set; }
    public List<IFormFile>? NewProductImages { get; set; }
    public List<string>? ExistingImageIds { get; set; }
    public int? ShowcaseImageIndex { get; set; }
    

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, UpdatedProductResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly IFeatureRepository _featureRepository;
        private readonly IImageFileRepository _imageFileRepository;
        private readonly ProductBusinessRules _productBusinessRules;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;

        public UpdateProductCommandHandler(IProductRepository productRepository, 
                                           IMapper mapper, 
                                           ProductBusinessRules productBusinessRules, 
                                           IFeatureRepository featureRepository,
                                           IStorageService storageService, IImageFileRepository imageFileRepository)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _productBusinessRules = productBusinessRules;
            _featureRepository = featureRepository;
            _storageService = storageService;
            _imageFileRepository = imageFileRepository;
        }

        public async Task<UpdatedProductResponse> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            Product? product = await _productRepository.GetAsync(
                p => p.Id == request.Id,
                include: p => p.Include(e => e.Category)
                    .Include(e => e.Brand)
                    .Include(e => e.ProductFeatureValues)
                    .Include(e => e.ProductImageFiles),
                cancellationToken: cancellationToken);

            await _productBusinessRules.ProductShouldExistWhenSelected(product);

            if (product != null)
            {
                // Update basic information
                product.Name = request.Name;
                product.Title = request.Title;
                product.Description = request.Description;
                product.CategoryId = request.CategoryId;
                product.BrandId = request.BrandId;
                product.VaryantGroupID = request.VaryantGroupID;
                product.Tax = request.Tax;
                product.Stock = request.Stock;
                product.Price = request.Price;
                product.Sku = request.Sku;

                // Update product features
                product.ProductFeatureValues.Clear();
                foreach (var featureDto in request.ProductFeatures)
                {
                    foreach (var featureValueDto in featureDto.FeatureValues)
                    {
                        product.ProductFeatureValues.Add(new ProductFeatureValue
                        {
                            ProductId = product.Id,
                            FeatureValueId = featureValueDto.Id
                        });
                    }
                }

                // Update product images
                if (request.ExistingImageIds != null)
                {
                    var imagesToRemove = product.ProductImageFiles.Where(pif => !request.ExistingImageIds.Contains(pif.Id)).ToList();
                    foreach (var imageToRemove in imagesToRemove)
                    {
                        product.ProductImageFiles.Remove(imageToRemove);
                        await _imageFileRepository.DeleteAsync(imageToRemove);
                        try
                        {
                            // Burada yeni DeleteFromAllStoragesAsync metodunu kullanıyoruz
                            await _storageService.DeleteFromAllStoragesAsync("products", imageToRemove.Path, imageToRemove.Name);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error deleting file from storages: {ex.Message}");
                        }
                    }
                }

                if (request.NewProductImages != null && request.NewProductImages.Any())
                {
                    var uploadedFiles = await _storageService.UploadAsync("products", product.Id, request.NewProductImages);
                    foreach (var file in uploadedFiles)
                    {
                        var productImageFile = new ProductImageFile(file.fileName, file.entityType, file.path, file.storageType);
                        product.ProductImageFiles?.Add(productImageFile);
                    }
                }

                // Set showcase image
                if (request.ShowcaseImageIndex.HasValue && request.ShowcaseImageIndex.Value < product.ProductImageFiles?.Count)
                {
                    foreach (var image in product.ProductImageFiles)
                    {
                        image.Showcase = false;
                    }
                    product.ProductImageFiles.ElementAt(request.ShowcaseImageIndex.Value).Showcase = true;
                }

                await _productRepository.UpdateAsync(product);

                UpdatedProductResponse response = _mapper.Map<UpdatedProductResponse>(product);
                return response;
            }

            throw new Exception("Product not found");
        }
    }
}