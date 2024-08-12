using Application.Services;
using MediatR;

namespace Application.Features.Carts.Commands.UpdateQuantity;

public class UpdateQuantityCommand : IRequest<UpdateQuantityResponse>
{
    public string CartItemId { get; set; }
    public int Quantity { get; set; }
    
    public class UpdateQuantityCommandHandler : IRequestHandler<UpdateQuantityCommand, UpdateQuantityResponse>
    {
        private readonly ICartService _cartService;

        public UpdateQuantityCommandHandler(ICartService cartService)
        {
            _cartService = cartService;
        }

        public async Task<UpdateQuantityResponse> Handle(UpdateQuantityCommand request, CancellationToken cancellationToken)
        {
            await _cartService.UpdateQuantityAsync(new()
            {
                CartItemId = request.CartItemId,
                Quantity = request.Quantity,
            });
            return new();
        }
    }
}