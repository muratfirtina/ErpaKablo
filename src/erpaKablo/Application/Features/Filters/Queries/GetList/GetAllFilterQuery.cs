using Application.Repositories;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Filters.Queries.GetList;

public class GetAllFilterQuery : IRequest<GetListResponse<GetAllFilterQueryResponse>>
{
    public PageRequest PageRequest { get; set; }
    
    public class GetAllFilterQueryHandler : IRequestHandler<GetAllFilterQuery, GetListResponse<GetAllFilterQueryResponse>>
    {
        private readonly IFilterRepository _filterRepository;
        private readonly IMapper _mapper;

        public GetAllFilterQueryHandler(IFilterRepository filterRepository, IMapper mapper)
        {
            _filterRepository = filterRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetAllFilterQueryResponse>> Handle(GetAllFilterQuery request, CancellationToken cancellationToken)
        {
            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                List<Filter> filters = await _filterRepository.GetAllAsync(
                    cancellationToken: cancellationToken);
                GetListResponse<GetAllFilterQueryResponse> response = _mapper.Map<GetListResponse<GetAllFilterQueryResponse>>(filters);
                return response;
            }
            else
            {
                IPaginate<Filter> filters = await _filterRepository.GetListAsync(
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    cancellationToken: cancellationToken
                );
                GetListResponse<GetAllFilterQueryResponse> response = _mapper.Map<GetListResponse<GetAllFilterQueryResponse>>(filters);
                return response;
            }
        }
    }
}