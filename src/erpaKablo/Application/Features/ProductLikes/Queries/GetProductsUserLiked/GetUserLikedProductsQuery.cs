using Application.Features.ProductImageFiles.Dtos;
using Application.Features.Products.Dtos;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;
using MediatR;

namespace Application.Features.ProductLikes.Queries.GetProductsUserLiked;

public class GetUserLikedProductsQuery : IRequest<GetListResponse<GetUserLikedProductsQueryResponse>>
{
    public PageRequest PageRequest { get; set; }

    public class GetUserLikedProductsQueryHandler : IRequestHandler<GetUserLikedProductsQuery, GetListResponse<GetUserLikedProductsQueryResponse>>
    {
        private readonly IProductLikeRepository _productLikeRepository;
        private readonly IStorageService _storageService;
        private readonly IMapper _mapper;

        public GetUserLikedProductsQueryHandler(IProductLikeRepository productLikeRepository, IMapper mapper, IStorageService storageService)
        {
            _productLikeRepository = productLikeRepository;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<GetListResponse<GetUserLikedProductsQueryResponse>> Handle(GetUserLikedProductsQuery request, CancellationToken cancellationToken)
        {
            IPaginate<ProductLike> productLikes = await _productLikeRepository.GetUserLikedProductsAsync(
                request.PageRequest.PageIndex,
                request.PageRequest.PageSize,
                cancellationToken
            );
            
            var response = _mapper.Map<GetListResponse<GetUserLikedProductsQueryResponse>>(productLikes);
            SetProductImageUrls(response);

            return response;
        }

        private void SetProductImageUrls(GetListResponse<GetUserLikedProductsQueryResponse> products)
        {
            var baseUrl = _storageService.GetStorageUrl();
            foreach (var product in products.Items)
            {
                if (product.ShowcaseImage == null)
                {
                    product.ShowcaseImage = new ProductImageFileDto
                    {
                        EntityType = "products",
                        Path = "",
                        FileName = "ecommerce-default-product.png",
                    };
                }

                product.ShowcaseImage.Url = product.ShowcaseImage.FileName == "ecommerce-default-product.png"
                    ? $"{baseUrl}{product.ShowcaseImage.EntityType}/{product.ShowcaseImage.FileName}"
                    : $"{baseUrl}{product.ShowcaseImage.EntityType}/{product.ShowcaseImage.Path}/{product.ShowcaseImage.FileName}";
            }
        }
    }
}