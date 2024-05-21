using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductFeatures.Queries.GetById;

public class GetByIdProductFeatureQuery : IRequest<GetByIdProductFeatureResponse>
{
    public string Id { get; set; }
    
    public class GetByIdProductFeatureQueryHandler : IRequestHandler<GetByIdProductFeatureQuery, GetByIdProductFeatureResponse>
    {
        private readonly IProductFeatureRepository _productFeatureRepository;
        private readonly IMapper _mapper;

        public GetByIdProductFeatureQueryHandler(IProductFeatureRepository productFeatureRepository, IMapper mapper)
        {
            _productFeatureRepository = productFeatureRepository;
            _mapper = mapper;
        }

        public async Task<GetByIdProductFeatureResponse> Handle(GetByIdProductFeatureQuery request, CancellationToken cancellationToken)
        {
            ProductFeature? productFeature = await _productFeatureRepository.GetAsync(
                predicate: p => p.Id == request.Id,
                cancellationToken: cancellationToken);
            GetByIdProductFeatureResponse response = _mapper.Map<GetByIdProductFeatureResponse>(productFeature);
            return response;
        }
    }
}