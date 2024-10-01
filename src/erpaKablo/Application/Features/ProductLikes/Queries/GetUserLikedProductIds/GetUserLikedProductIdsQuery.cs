using Application.Repositories;
using MediatR;

namespace Application.Features.ProductLikes.Queries.GetUserLikedProductIds;

public class GetUserLikedProductIdsQuery : IRequest<GetUserLikedProductIdsResponse>
{
    public string SearchProductIds { get; set; }

    public class GetUserLikedProductIdsQueryHandler : IRequestHandler<GetUserLikedProductIdsQuery, GetUserLikedProductIdsResponse>
    {
        private readonly IProductLikeRepository _productLikeRepository;

        public GetUserLikedProductIdsQueryHandler(IProductLikeRepository productLikeRepository)
        {
            _productLikeRepository = productLikeRepository;
        }

        public async Task<GetUserLikedProductIdsResponse> Handle(GetUserLikedProductIdsQuery request, CancellationToken cancellationToken)
        {
            var likedProductIds = await _productLikeRepository.GetUserLikedProductIdsAsync(request.SearchProductIds);
            return new GetUserLikedProductIdsResponse { LikedProductIds = likedProductIds };
        }
    }
}