using Application.Extensions;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetMostLikedProducts;

public class GetMostLikedProductQuery : IRequest<GetListResponse<GetMostLikedProductQueryResponse>>
{
    public int Count { get; set; } = 10;
    
    public class GetMostLikedProductQueryHandler : IRequestHandler<GetMostLikedProductQuery, GetListResponse<GetMostLikedProductQueryResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IStorageService _storageService;
        private readonly IMapper _mapper;


        public GetMostLikedProductQueryHandler(IProductRepository productRepository, IStorageService storageService, IMapper mapper)
        {
            _productRepository = productRepository;
            _storageService = storageService;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetMostLikedProductQueryResponse>> Handle(GetMostLikedProductQuery request, CancellationToken cancellationToken)
        {
            //ençok beğenilen 10 ürünü getir.
            IPaginate<Product> product = await _productRepository.GetListAsync(
                predicate: x => x.ProductLikes.Count > 0,
                include: x => x
                    .Include(x => x.Category)
                    .Include(x => x.Brand)
                    .Include(x => x.ProductImageFiles.Where(pif => pif.Showcase == true))
                    .Include(x => x.ProductFeatureValues).ThenInclude(x => x.FeatureValue).ThenInclude(x => x.Feature));
            
            var mostLikedProducts = product.Items               
                .OrderByDescending(x => x.ProductLikes.Count)
                .Take(request.Count)
                .ToList();
            
            GetListResponse<GetMostLikedProductQueryResponse> response = _mapper.Map<GetListResponse<GetMostLikedProductQueryResponse>>(mostLikedProducts);
            response.Items.SetImageUrls(_storageService);
            return response;

        }
    }
    
}
