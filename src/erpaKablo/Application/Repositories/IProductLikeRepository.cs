using Core.Persistence.Repositories;
using Domain;

namespace Application.Repositories;

public interface IProductLikeRepository : IAsyncRepository<ProductLike, string>, IRepository<ProductLike, string>
{
    
}