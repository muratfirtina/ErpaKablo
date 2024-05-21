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
    //public ICollection<ProductFeatureDto>? ProductFeatures { get; set; }

    public class UpdateProductCommandHandler : IRequestHandler<UpdateProductCommand, UpdatedProductResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly ProductBusinessRules _productBusinessRules;
        private readonly IMapper _mapper;

        public UpdateProductCommandHandler(IProductRepository productRepository, IMapper mapper, ProductBusinessRules productBusinessRules)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _productBusinessRules = productBusinessRules;
        }

        public async Task<UpdatedProductResponse> Handle(UpdateProductCommand request,
            CancellationToken cancellationToken)
        {
            Product? product = await _productRepository.GetAsync(p => p.Id == request.Id,
                include: p => p.Include(e => e.Category)
                    .Include(e => e.Brand)
                    .Include(e => e.ProductFeatures)!.ThenInclude(e => e.Features),
                cancellationToken: cancellationToken);
            await _productBusinessRules.ProductShouldExistWhenSelected(product);
            if (product != null)
            {
                product.CategoryId = request.CategoryId;
                product.BrandId = request.BrandId;
                product = _mapper.Map(request, product);
                await _productRepository.UpdateAsync(product);
                UpdatedProductResponse response = _mapper.Map<UpdatedProductResponse>(product);
                return response;
            }
            throw new Exception("Product not found");
        }
    }
}