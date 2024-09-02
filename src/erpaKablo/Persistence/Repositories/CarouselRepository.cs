using Application.Repositories;
using Core.Persistence.Repositories;
using Domain;
using Persistence.Context;

namespace Persistence.Repositories;

public class CarouselRepository : EfRepositoryBase<Carousel, string, ErpaKabloDbContext>, ICarouselRepository
{
    public CarouselRepository(ErpaKabloDbContext context) : base(context)
    {
    }
}