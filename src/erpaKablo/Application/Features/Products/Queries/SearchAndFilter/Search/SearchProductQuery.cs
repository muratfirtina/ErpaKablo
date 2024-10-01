using Application.Extensions;
using Application.Features.ProductImageFiles.Dtos;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;
using MediatR;

namespace Application.Features.Products.Queries.SearchAndFilter.Search;

public class SearchProductQuery: IRequest<GetListResponse<SearchProductQueryResponse>>
{
    public string SearchTerm { get; set; }
    public PageRequest PageRequest { get; set; }
    
    public class SearchProductQueryHandler : IRequestHandler<SearchProductQuery, GetListResponse<SearchProductQueryResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductLikeRepository _productLikeRepository;
        private readonly IStorageService _storageService;
        private readonly IMapper _mapper;

        public SearchProductQueryHandler(IProductRepository productRepository, IStorageService storageService, IMapper mapper, IProductLikeRepository productLikeRepository)
        {
            _productRepository = productRepository;
            _storageService = storageService;
            _mapper = mapper;
            _productLikeRepository = productLikeRepository;
        }

        public async Task<GetListResponse<SearchProductQueryResponse>> Handle(SearchProductQuery request, CancellationToken cancellationToken)
        {
            IPaginate<Product> products = await _productRepository.SearchProductsAsync(request.SearchTerm, request.PageRequest.PageIndex, request.PageRequest.PageSize);
            var productDtos = _mapper.Map<GetListResponse<SearchProductQueryResponse>>(products);
            productDtos.Items.SetImageUrls(_storageService);
            return productDtos;
        }
        
    }
    
}
    
