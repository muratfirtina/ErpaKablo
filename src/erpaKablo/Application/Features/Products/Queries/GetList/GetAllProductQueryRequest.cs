using Application.Features.ProductImageFiles.Dtos;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetList;

public class GetAllProductQuery : IRequest<GetListResponse<GetAllProductQueryResponse>>
{
    public PageRequest PageRequest { get; set; }

    public class
        GetAllProductQueryHandler : IRequestHandler<GetAllProductQuery, GetListResponse<GetAllProductQueryResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IStorageService _storageService;
        private readonly IMapper _mapper;

        public GetAllProductQueryHandler(IProductRepository productRepository, IMapper mapper,
            IStorageService storageService)
        {
            _productRepository = productRepository;

            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<GetListResponse<GetAllProductQueryResponse>> Handle(GetAllProductQuery request,
            CancellationToken cancellationToken)
        {
            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                List<Product> products = await _productRepository.GetAllAsync(
                    include: p => p
                        .Include(p => p.Category)
                        .Include(p => p.Brand)
                        .Include(x => x.ProductFeatureValues).ThenInclude(x => x.FeatureValue)
                        .ThenInclude(x => x.Feature)
                        .Include(x => x.ProductImageFiles.Where(pif => pif.Showcase == true)),
                    cancellationToken: cancellationToken);

                GetListResponse<GetAllProductQueryResponse> response =
                    _mapper.Map<GetListResponse<GetAllProductQueryResponse>>(products);

                SetProductImageUrls(response.Items);

                return response;
            }
            else
            {
                IPaginate<Product> products = await _productRepository.GetListAsync(
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    include: x => x.Include(x => x.Category)
                        .Include(x => x.Brand)
                        .Include(x => x.ProductFeatureValues).ThenInclude(x => x.FeatureValue)
                        .ThenInclude(x => x.Feature)
                        .Include(x => x.ProductImageFiles.Where(pif => pif.Showcase == true)),
                    cancellationToken: cancellationToken
                );
                GetListResponse<GetAllProductQueryResponse> response =
                    _mapper.Map<GetListResponse<GetAllProductQueryResponse>>(products);

                SetProductImageUrls(response.Items);
                return response;
            }
        }

        private void SetProductImageUrls(IEnumerable<GetAllProductQueryResponse> products)
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