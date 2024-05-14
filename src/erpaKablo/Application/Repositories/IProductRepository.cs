using Application.Features.Products.Dtos;
using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IProductRepository : IAsyncRepository<Product, int>, IRepository<Product, int>
{
    Task<List<GetProductImageFileDto>> GetFilesByProductId(string productId);
    Task ChangeShowcase(string productId, string imageFileId,bool showcase);
    Task<ProductImageFile?> GetProductImage(string productId);
}