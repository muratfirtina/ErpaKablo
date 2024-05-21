using Application.Repositories;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Features.Queries.GetList;

public class GetAllFeatureQuery : IRequest<GetListResponse<GetAllFeatureQueryResponse>>
{
    public PageRequest PageRequest { get; set; }
    
    public class GetAllFeatureQueryHandler : IRequestHandler<GetAllFeatureQuery, GetListResponse<GetAllFeatureQueryResponse>>
    {
        private readonly IFeatureRepository _featureRepository;
        private readonly IMapper _mapper;

        public GetAllFeatureQueryHandler(IFeatureRepository featureRepository, IMapper mapper)
        {
            _featureRepository = featureRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetAllFeatureQueryResponse>> Handle(GetAllFeatureQuery request, CancellationToken cancellationToken)
        {
            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                List<Feature> features = await _featureRepository.GetAllAsync(
                    cancellationToken: cancellationToken);
                GetListResponse<GetAllFeatureQueryResponse> response = _mapper.Map<GetListResponse<GetAllFeatureQueryResponse>>(features);
                return response;
            }
            else
            {
                IPaginate<Feature> features = await _featureRepository.GetListAsync(
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    cancellationToken: cancellationToken
                );
                GetListResponse<GetAllFeatureQueryResponse> response = _mapper.Map<GetListResponse<GetAllFeatureQueryResponse>>(features);
                return response;
            }
        }
    }
}