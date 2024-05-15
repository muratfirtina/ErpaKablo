using Application.Features.Products.Consts;
using Application.Repositories;
using Core.Application.Rules;
using Core.CrossCuttingConcerns.Exceptions;
using Domain;

namespace Application.Features.Products.Rules;

public class ProductBusinessRules : BaseBusinessRules
{
    private readonly IProductRepository _productRepository;

    public ProductBusinessRules(IProductRepository productRepository)
    {
        _productRepository = productRepository;
    }

    public Task ProductShouldExistWhenSelected(Product? product)
    {
        if (product == null)
            throw new BusinessException(ProductsBusinessMessages.ProductNotExists);
        return Task.CompletedTask;
    }

    public async Task ProductIdShouldExistWhenSelected(string id, CancellationToken cancellationToken)
    {
        Product? product = await _productRepository.GetAsync(
            predicate: e => e.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        await ProductShouldExistWhenSelected(product);
    }
    
}