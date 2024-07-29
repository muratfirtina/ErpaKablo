using Application.Features.ProductImageFiles.Dtos;
using Application.Features.Products.Dtos;
using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IProductRepository : IAsyncRepository<Product, string>, IRepository<Product, string>
{
    Task<List<ProductImageFileDto>> GetFilesByProductId(string productId);
    Task ChangeShowcase(string productId, string imageFileId,bool showcase);
    Task<ProductImageFile?> GetProductImage(string productId);
}