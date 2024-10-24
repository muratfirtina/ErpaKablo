using Application.Extensions;
using Application.Features.ProductImageFiles.Dtos;
using Application.Features.Products.Dtos;
using Application.Services;
using Application.Storage;
using Core.Application.Responses;
using MediatR;

namespace Application.Features.Carts.Queries.GetList;

public class GetCartItemsQuery :IRequest<List<GetCartItemsQueryResponse>>
{
    public class GetCartItemsQueryHandler : IRequestHandler<GetCartItemsQuery, List<GetCartItemsQueryResponse>>
    {
        private readonly ICartService _cartService;
        private readonly IStorageService _storageService;

        public GetCartItemsQueryHandler(ICartService cartService, IStorageService storageService)
        {
            _cartService = cartService;
            _storageService = storageService;
        }

        public async Task<List<GetCartItemsQueryResponse>> Handle(GetCartItemsQuery request, CancellationToken cancellationToken)
        {
            var cartItems = await _cartService.GetCartItemsAsync();
            var response = cartItems.Select(ci => new GetCartItemsQueryResponse
            {
                CartItemId = ci.Id.ToString(),
                ProductName = ci.Product.Name,
                BrandName = ci.Product.Brand.Name,
                ProductFeatureValues =
                    ci.Product.ProductFeatureValues.Select(pfv => new ProductFeatureValueDto()
                    {
                        FeatureName = pfv.FeatureValue.Feature.Name,
                        FeatureValueName = pfv.FeatureValue.Name
                    }).ToList(),
                Quantity = ci.Quantity,
                ShowcaseImage = new ProductImageFileDto()
                {
                    
                    EntityType = ci.Product.ProductImageFiles.FirstOrDefault(pif => pif.Showcase)?.EntityType,
                    Path = ci.Product.ProductImageFiles.FirstOrDefault(pif => pif.Showcase)?.Path,
                    FileName = ci.Product.ProductImageFiles.FirstOrDefault(pif => pif.Showcase)?.Name,
                    Url = ci.Product.ProductImageFiles.FirstOrDefault(pif => pif.Showcase)?.Url
                },
                UnitPrice = ci.Product.Price,
                IsChecked = ci.IsChecked
            }).ToList();

            response.SetImageUrls(_storageService);
            return response;
        }
        
    }
}