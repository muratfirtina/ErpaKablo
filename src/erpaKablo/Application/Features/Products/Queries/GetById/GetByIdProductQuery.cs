using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetById;

public class GetByIdProductQuery : IRequest<GetByIdProductResponse>
{
    public string Id { get; set; }
    
    public class GetByIdProductQueryHandler : IRequestHandler<GetByIdProductQuery, GetByIdProductResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetByIdProductQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<GetByIdProductResponse> Handle(GetByIdProductQuery request, CancellationToken cancellationToken)
        {
            Product product = await _productRepository.GetAsync(
                predicate: p => p.Id == request.Id,
                cancellationToken: cancellationToken,
                include: x => x.Include(x => x.Category)
                    .Include(x => x.Brand)
                    .Include(x => x.ProductImageFiles));
            GetByIdProductResponse response = _mapper.Map<GetByIdProductResponse>(product);
            return response;
        }
    }
}