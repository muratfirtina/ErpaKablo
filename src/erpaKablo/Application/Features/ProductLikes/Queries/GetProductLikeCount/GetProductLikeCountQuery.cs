using Application.Repositories;
using MediatR;

namespace Application.Features.ProductLikes.Queries.GetProductLikeCount;

public class GetProductLikeCountQuery : IRequest<GetProductLikeCountQueryResponse>
{
    public string ProductId { get; set; }
    
    public class GetProductLikeCountQueryHandler : IRequestHandler<GetProductLikeCountQuery, GetProductLikeCountQueryResponse>
    {
        private readonly IProductLikeRepository _productLikeRepository;

        public GetProductLikeCountQueryHandler(IProductLikeRepository productLikeRepository)
        {
            _productLikeRepository = productLikeRepository;
        }

        public async Task<GetProductLikeCountQueryResponse> Handle(GetProductLikeCountQuery request, CancellationToken cancellationToken)
        {
            var likeCount = await _productLikeRepository.GetProductLikeCountAsync(request.ProductId);
            return new GetProductLikeCountQueryResponse { LikeCount = likeCount };
        }
    }
}