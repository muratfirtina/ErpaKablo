using Application.Features.ProductImageFiles.Dtos;
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

            SetProductImageUrls(response);
            return response;
        }
    
        private void SetProductImageUrls(List<GetCartItemsQueryResponse> cartItems)
        {
            var baseUrl = _storageService.GetStorageUrl();
            if (string.IsNullOrEmpty(baseUrl)) return;
            foreach (var cartItem in cartItems)
            {
                if (cartItem.ShowcaseImage != null)
                {
                    cartItem.ShowcaseImage.Url = cartItem.ShowcaseImage.FileName == "ecommerce-default-product.png"
                        ? $"{baseUrl}{cartItem.ShowcaseImage.EntityType}/{cartItem.ShowcaseImage.FileName}"
                        : $"{baseUrl}{cartItem.ShowcaseImage.EntityType}/{cartItem.ShowcaseImage.Path}/{cartItem.ShowcaseImage.FileName}";
                }
            }
        }
    }
}