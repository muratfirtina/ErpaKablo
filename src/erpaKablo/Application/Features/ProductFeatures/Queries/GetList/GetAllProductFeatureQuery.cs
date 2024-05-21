using Application.Repositories;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductFeatures.Queries.GetList;

public class GetAllProductFeatureQuery : IRequest<GetListResponse<GetAllProductFeatureQueryResponse>>
{
    public PageRequest PageRequest { get; set; }
    
    public class GetAllProductFeatureQueryHandler : IRequestHandler<GetAllProductFeatureQuery, GetListResponse<GetAllProductFeatureQueryResponse>>
    {
        private readonly IProductFeatureRepository _productFeatureRepository;
        private readonly IMapper _mapper;

        public GetAllProductFeatureQueryHandler(IProductFeatureRepository productFeatureRepository, IMapper mapper)
        {
            _productFeatureRepository = productFeatureRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetAllProductFeatureQueryResponse>> Handle(GetAllProductFeatureQuery request, CancellationToken cancellationToken)
        {
            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                List<ProductFeature> productFeatures = await _productFeatureRepository.GetAllAsync(
                    cancellationToken: cancellationToken);
                GetListResponse<GetAllProductFeatureQueryResponse> response = _mapper.Map<GetListResponse<GetAllProductFeatureQueryResponse>>(productFeatures);
                return response;
            }
            else
            {
                IPaginate<ProductFeature> productFeatures = await _productFeatureRepository.GetListAsync(
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    cancellationToken: cancellationToken
                );
                GetListResponse<GetAllProductFeatureQueryResponse> response = _mapper.Map<GetListResponse<GetAllProductFeatureQueryResponse>>(productFeatures);
                return response;
            }
        }
    }
}