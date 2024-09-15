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
        private readonly IStorageService _storageService;
        private readonly IMapper _mapper;

        public SearchProductQueryHandler(IProductRepository productRepository, IStorageService storageService, IMapper mapper)
        {
            _productRepository = productRepository;
            _storageService = storageService;
            _mapper = mapper;
        }

        public async Task<GetListResponse<SearchProductQueryResponse>> Handle(SearchProductQuery request, CancellationToken cancellationToken)
        {
            IPaginate<Product> products = await _productRepository.SearchProductsAsync(request.SearchTerm, request.PageRequest.PageIndex, request.PageRequest.PageSize);
            var productDtos = _mapper.Map<GetListResponse<SearchProductQueryResponse>>(products);
            SetProductImageUrls(productDtos.Items);
            return productDtos;
        }
        
        private void SetProductImageUrls(IEnumerable<SearchProductQueryResponse> products)
        {
            var baseUrl = _storageService.GetStorageUrl();
            foreach (var product in products)
            {
                if (product.ShowcaseImage == null)
                {
                    // Eğer ShowcaseImage null ise, varsayılan bir ProductImageFileDto oluştur
                    product.ShowcaseImage = new ProductImageFileDto
                    {
                        EntityType = "products",
                        Path = "",
                        FileName = "ecommerce-default-product.png"
                    };
                }

                // Her durumda URL'yi ayarla
                product.ShowcaseImage.Url = product.ShowcaseImage.FileName == "ecommerce-default-product.png"
                    ? $"{baseUrl}{product.ShowcaseImage.EntityType}/{product.ShowcaseImage.FileName}"
                    : $"{baseUrl}{product.ShowcaseImage.EntityType}/{product.ShowcaseImage.Path}/{product.ShowcaseImage.FileName}";
            }
        }
    }
    
}
    
