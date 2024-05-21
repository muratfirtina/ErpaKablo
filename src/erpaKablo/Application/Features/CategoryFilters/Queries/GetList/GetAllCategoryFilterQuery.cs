using Application.Repositories;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CategoryFilters.Queries.GetList;

public class GetAllCategoryFilterQuery : IRequest<GetListResponse<GetAllCategoryFilterQueryResponse>>
{
    public PageRequest PageRequest { get; set; }
    
    public class GetAllCategoryFilterQueryHandler : IRequestHandler<GetAllCategoryFilterQuery, GetListResponse<GetAllCategoryFilterQueryResponse>>
    {
        private readonly ICategoryFilterRepository _categoryFilterRepository;
        private readonly IMapper _mapper;

        public GetAllCategoryFilterQueryHandler(ICategoryFilterRepository categoryFilterRepository, IMapper mapper)
        {
            _categoryFilterRepository = categoryFilterRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetAllCategoryFilterQueryResponse>> Handle(GetAllCategoryFilterQuery request, CancellationToken cancellationToken)
        {
            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                List<CategoryFilter> categoryFilters = await _categoryFilterRepository.GetAllAsync(
                    cancellationToken: cancellationToken);
                GetListResponse<GetAllCategoryFilterQueryResponse> response = _mapper.Map<GetListResponse<GetAllCategoryFilterQueryResponse>>(categoryFilters);
                return response;
            }
            else
            {
                IPaginate<CategoryFilter> categoryFilters = await _categoryFilterRepository.GetListAsync(
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    cancellationToken: cancellationToken
                );
                GetListResponse<GetAllCategoryFilterQueryResponse> response = _mapper.Map<GetListResponse<GetAllCategoryFilterQueryResponse>>(categoryFilters);
                return response;
            }
        }
    }
}