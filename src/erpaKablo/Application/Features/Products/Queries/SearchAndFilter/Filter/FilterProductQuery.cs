using Application.Features.ProductImageFiles.Dtos;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;
using MediatR;

namespace Application.Features.Products.Queries.SearchAndFilter.Filter;

public class FilterProductWithPaginationQuery : IRequest<GetListResponse<FilterProductQueryResponse>>
{
    public string SearchTerm { get; set; }
    public PageRequest PageRequest { get; set; }
    public Dictionary<string, List<string>> Filters { get; set; }

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
                request.PageRequest
            );
            GetListResponse<FilterProductQueryResponse> response = _mapper.Map<GetListResponse<FilterProductQueryResponse>>(products);
            
            SetProductImageUrls(response.Items);
            
            return response;
        }

        private void SetProductImageUrls(IEnumerable<FilterProductQueryResponse> products)
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

public class FilterProductQuery : IRequest<GetListResponse<FilterProductQueryResponse>>
{
    public string SearchTerm { get; set; }
    public Dictionary<string, List<string>> Filters { get; set; }
}