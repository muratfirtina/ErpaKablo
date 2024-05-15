using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class ImageFileRepository : EfRepositoryBase<ImageFile, string, ErpaKabloDbContext>, IImageFileRepository
{
    public ImageFileRepository(ErpaKabloDbContext context) : base(context)
    {
    }
    public async Task AddAsync(List<ProductImageFile> toList)
    {
        await Context.Set<ProductImageFile>().AddRangeAsync(toList);
        await Context.SaveChangesAsync();
        
    }
}