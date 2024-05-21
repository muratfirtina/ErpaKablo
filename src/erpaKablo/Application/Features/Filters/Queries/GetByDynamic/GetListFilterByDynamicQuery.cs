using Application.Features.Filters.Rules;
using Application.Repositories;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Filter = Domain.Filter;

namespace Application.Features.Filters.Queries.GetByDynamic;

public class GetListFilterByDynamicQuery : IRequest<GetListResponse<GetListFilterByDynamicDto>>
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery DynamicQuery { get; set; }
    
    public class GetListByDynamicFilterQueryHandler : IRequestHandler<GetListFilterByDynamicQuery, GetListResponse<GetListFilterByDynamicDto>>
    {
        private readonly IFilterRepository _filterRepository;
        private readonly IMapper _mapper;
        private readonly FilterBusinessRules _filterBusinessRules;

        public GetListByDynamicFilterQueryHandler(IFilterRepository filterRepository, IMapper mapper, FilterBusinessRules filterBusinessRules)
        {
            _filterRepository = filterRepository;
            _mapper = mapper;
            _filterBusinessRules = filterBusinessRules;
        }

        public async Task<GetListResponse<GetListFilterByDynamicDto>> Handle(GetListFilterByDynamicQuery request, CancellationToken cancellationToken)
        {
            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                var allFilters = await _filterRepository.GetAllByDynamicAsync(
                    request.DynamicQuery,
                    cancellationToken: cancellationToken);

                var filtersDtos = _mapper.Map<GetListResponse<GetListFilterByDynamicDto>>(allFilters);
                return filtersDtos;
            }
            else
            {
                IPaginate<Filter> filters = await _filterRepository.GetListByDynamicAsync(
                    request.DynamicQuery,
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    cancellationToken: cancellationToken);
                
                var filtersDtos = _mapper.Map<GetListResponse<GetListFilterByDynamicDto>>(filters);
                return filtersDtos;

            }
        }
    }
}