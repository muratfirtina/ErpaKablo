using Application.Repositories;
using MediatR;

namespace Application.Features.ProductLikes.Queries.IsProductLiked;

public class IsProductLikedQuery : IRequest<IsProductLikedQueryResponse>
{
    public string ProductId { get; set; }
    
    public class IsProductLikedQueryHandler : IRequestHandler<IsProductLikedQuery, IsProductLikedQueryResponse>
    {
        private readonly IProductLikeRepository _productLikeRepository;

        public IsProductLikedQueryHandler(IProductLikeRepository productLikeRepository)
        {
            _productLikeRepository = productLikeRepository;
        }

        public async Task<IsProductLikedQueryResponse> Handle(IsProductLikedQuery request, CancellationToken cancellationToken)
        {
            var isLiked = await _productLikeRepository.IsProductLikedAsync(request.ProductId);
            return new IsProductLikedQueryResponse { IsLiked = isLiked };
        }
    }
}