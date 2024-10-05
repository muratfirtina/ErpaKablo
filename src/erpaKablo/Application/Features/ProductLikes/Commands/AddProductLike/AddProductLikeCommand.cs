using Application.Repositories;
using MediatR;

namespace Application.Features.ProductLikes.Commands.AddProductLike;

public class AddProductLikeCommand : IRequest<AddProductLikeResponse>
{
    public string ProductId { get; set; }
    
    public class AddProductLikeCommandHandler : IRequestHandler<AddProductLikeCommand, AddProductLikeResponse>
    {
        private readonly IProductLikeRepository _productLikeRepository;

        public AddProductLikeCommandHandler(IProductLikeRepository productLikeRepository)
        {
            _productLikeRepository = productLikeRepository;
        }

        public async Task<AddProductLikeResponse> Handle(AddProductLikeCommand request, CancellationToken cancellationToken)
        {
            var isLiked = await _productLikeRepository.IsProductLikedAsync(request.ProductId);

            if (isLiked)
            {
                var removed = await _productLikeRepository.RemoveProductLikeAsync(request.ProductId);
                return new AddProductLikeResponse { IsLiked = false };
            }
            else
            {
                await _productLikeRepository.AddProductLikeAsync(request.ProductId);
                return new AddProductLikeResponse { IsLiked = true };
            }
        }
    }
}