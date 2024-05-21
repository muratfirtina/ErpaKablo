using Application.Features.CategoryFilters.Rules;
using Application.Repositories;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CategoryFilters.Queries.GetByDynamic;

public class GetListCategoryFilterByDynamicQuery : IRequest<GetListResponse<GetListCategoryFilterByDynamicDto>>
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery DynamicQuery { get; set; }
    
    public class GetListByDynamicCategoryFilterQueryHandler : IRequestHandler<GetListCategoryFilterByDynamicQuery, GetListResponse<GetListCategoryFilterByDynamicDto>>
    {
        private readonly ICategoryFilterRepository _categoryFilterRepository;
        private readonly IMapper _mapper;
        private readonly CategoryFilterBusinessRules _categoryFilterBusinessRules;

        public GetListByDynamicCategoryFilterQueryHandler(ICategoryFilterRepository categoryFilterRepository, IMapper mapper, CategoryFilterBusinessRules categoryFilterBusinessRules)
        {
            _categoryFilterRepository = categoryFilterRepository;
            _mapper = mapper;
            _categoryFilterBusinessRules = categoryFilterBusinessRules;
        }

        public async Task<GetListResponse<GetListCategoryFilterByDynamicDto>> Handle(GetListCategoryFilterByDynamicQuery request, CancellationToken cancellationToken)
        {
            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                var allCategoryFilters = await _categoryFilterRepository.GetAllByDynamicAsync(
                    request.DynamicQuery,
                    cancellationToken: cancellationToken);

                var categoryFiltersDtos = _mapper.Map<GetListResponse<GetListCategoryFilterByDynamicDto>>(allCategoryFilters);
                return categoryFiltersDtos;
            }
            else
            {
                IPaginate<CategoryFilter> categoryFilters = await _categoryFilterRepository.GetListByDynamicAsync(
                    request.DynamicQuery,
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    cancellationToken: cancellationToken);
                
                var categoryFiltersDtos = _mapper.Map<GetListResponse<GetListCategoryFilterByDynamicDto>>(categoryFilters);
                return categoryFiltersDtos;

            }
        }
    }
}