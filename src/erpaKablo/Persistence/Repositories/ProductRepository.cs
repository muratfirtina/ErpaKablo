using Application.Features.Products.Dtos;
using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Microsoft.EntityFrameworkCore;
using Persistence.Context;

namespace Persistence.Repositories;

public class ProductRepository : EfRepositoryBase<Product, string, ErpaKabloDbContext>, IProductRepository
{
    public ProductRepository(ErpaKabloDbContext context) : base(context)
    {
    }

    public async Task<List<GetProductImageFileDto>> GetFilesByProductId(string productId)
    {
        //burada gelen product id ye göre imagefile tablosundan product id ye göre productimagefile ları getirip dönüş yapılacak.
        var query = Context.Products
            .Where(p => p.Id == productId)
            .SelectMany(p => p.ProductImageFiles)
            .OrderByDescending(e => e.CreatedDate)
            .Select(pif => new GetProductImageFileDto
            {
                Id = pif.Id,
                Path = pif.Path,
                FileName = pif.Name,
                Showcase = pif.Showcase,
                Storage = pif.Storage,
                Category = pif.Category
            }).ToListAsync();

        return await query;
            
    }

    public async Task ChangeShowcase(string productId, string imageFileId, bool showcase)
    {
        //burada seçili product id de ki hangi productimagefile seçiliyse onun showcase değeri true il değiştirilecek diğerleri false olacak.
        var product = await Context.Products.FindAsync(int.Parse(productId));
        var productImageFile = await Context.ProductImageFiles
            .Where(pif => pif.Id == imageFileId)
            .FirstOrDefaultAsync();
        if (productImageFile != null)
        {
            productImageFile.Showcase = showcase;
            await Context.SaveChangesAsync();
        }
        
    }

    public async Task<ProductImageFile?> GetProductImage(string productId)
    {
        //burada gelen product id ye göre productimagefile tablosundan product id ye göre productimagefile getirilecek.
        var query = Context.Products
            .Where(p => p.Id == productId)
            .SelectMany(p => p.ProductImageFiles)
            .FirstOrDefaultAsync();
        
        return await query;
    }
}