using Application.Services;
using Core.Application.Responses;
using MediatR;

namespace Application.Features.Carts.Queries.GetList;

public class GetCartItemsQuery :IRequest<List<GetCartItemsQueryResponse>>
{
    public class GetCartItemsQueryHandler: IRequestHandler<GetCartItemsQuery,List<GetCartItemsQueryResponse>>
    {
        private readonly ICartService _cartService;

        public GetCartItemsQueryHandler(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<List<GetCartItemsQueryResponse>> Handle(GetCartItemsQuery request, CancellationToken cancellationToken)
        {
            var cartItems = await _cartService.GetCartItemsAsync();
            return cartItems.Select(ci => new GetCartItemsQueryResponse
            {
                CartItemId = ci.Id.ToString(),
                ProductName = ci.Product.Name,
                Quantity = ci.Quantity,
                UnitPrice = ci.Product.Price,
                ProductImageUrls = ci.Product.ProductImageFiles.Select(pif => pif.Path).ToList(),
                IsChecked = ci.IsChecked
            
            }).ToList();
        }
    }
}