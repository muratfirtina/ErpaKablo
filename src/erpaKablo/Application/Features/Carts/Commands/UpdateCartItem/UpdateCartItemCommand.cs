using Application.Features.Carts.Dtos;
using Application.Services;
using MediatR;

namespace Application.Features.Carts.Commands.UpdateCartItem;

public class UpdateCartItemCommand:IRequest<UpdateCartItemResponse>
{
    public string CartItemId { get; set; }
    public bool IsChecked { get; set; }=true;
    
    public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, UpdateCartItemResponse>
    {
        private readonly ICartService _cartService;

        public UpdateCartItemCommandHandler(ICartService cartService)
        {
            _cartService = cartService;
        }
        public async Task<UpdateCartItemResponse> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
        {
            await _cartService.UpdateCartItemAsync(new()
            {
                CartItemId = request.CartItemId,
                IsChecked = request.IsChecked
            });
            return new();
        }
    }
}