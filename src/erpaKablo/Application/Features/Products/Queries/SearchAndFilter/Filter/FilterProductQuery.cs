using Application.Extensions;
using Application.Extensions.ImageFileExtensions;
using Application.Features.ProductImageFiles.Dtos;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.Application.Pipelines.Caching;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;
using MediatR;

namespace Application.Features.Products.Queries.SearchAndFilter.Filter;

public class FilterProductWithPaginationQuery : IRequest<GetListResponse<FilterProductQueryResponse>>, ICachableRequest
{
    public string SearchTerm { get; set; }
    public PageRequest PageRequest { get; set; }
    public Dictionary<string, List<string>> Filters { get; set; }
    public string SortOrder { get; set; }  = "default";
    
    public string CacheKey => $"FilterProductWithPaginationQuery_{SearchTerm}_{PageRequest.PageIndex}_{PageRequest.PageSize}_{SortOrder}";
    public bool BypassCache => false;
    public string? CacheGroupKey => "Products";
    public TimeSpan? SlidingExpiration => TimeSpan.FromMinutes(2);

    public class FilterProductQueryHandler : IRequestHandler<FilterProductWithPaginationQuery, GetListResponse<FilterProductQueryResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IStorageService _storageService;
        private readonly IMapper _mapper;

        public FilterProductQueryHandler(IProductRepository productRepository, IStorageService storageService, IMapper mapper)
        {
            _productRepository = productRepository;
            _storageService = storageService;
            _mapper = mapper;
        }  

        public async Task<GetListResponse<FilterProductQueryResponse>> Handle(FilterProductWithPaginationQuery request, CancellationToken cancellationToken)
        {
            IPaginate<Product> products = await _productRepository.FilterProductsAsync(
                request.SearchTerm, 
                request.Filters, 
                request.PageRequest,
                request.SortOrder
            );
            GetListResponse<FilterProductQueryResponse> response = _mapper.Map<GetListResponse<FilterProductQueryResponse>>(products);
            
            foreach (var productDto in response.Items)
            {
                var product = products.Items.First(p => p.Id == productDto.Id);
                var showcaseImage = product.ProductImageFiles?.FirstOrDefault(pif => pif.Showcase);
                if (showcaseImage != null)
                {
                    productDto.ShowcaseImage = showcaseImage.ToDto(_storageService);
                }
            }
            
            return response;
        }

        
    }
}

public class FilterProductQuery : IRequest<GetListResponse<FilterProductQueryResponse>>
{
    public string SearchTerm { get; set; }
    public Dictionary<string, List<string>> Filters { get; set; }
    public string SortOrder { get; set; } = "default";
}