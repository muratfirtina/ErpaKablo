using Application.Features.Categories.Rules;
using Application.Repositories;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Queries.GetByDynamic;

public class GetListCategoryByDynamicQuery : IRequest<GetListResponse<GetListCategoryByDynamicDto>>
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery DynamicQuery { get; set; }
    
    public class GetListByDynamicCategoryQueryHandler : IRequestHandler<GetListCategoryByDynamicQuery, GetListResponse<GetListCategoryByDynamicDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly CategoryBusinessRules _categoryBusinessRules;

        public GetListByDynamicCategoryQueryHandler(ICategoryRepository categoryRepository, IMapper mapper, CategoryBusinessRules categoryBusinessRules)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _categoryBusinessRules = categoryBusinessRules;
        }

        public async Task<GetListResponse<GetListCategoryByDynamicDto>> Handle(GetListCategoryByDynamicQuery request, CancellationToken cancellationToken)
        {
            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                var allCategories = await _categoryRepository.GetAllByDynamicAsync(
                    request.DynamicQuery,
                    cancellationToken: cancellationToken);

                var categoriesDtos = _mapper.Map<GetListResponse<GetListCategoryByDynamicDto>>(allCategories);
                return categoriesDtos;
            }
            else
            {
                IPaginate<Category> categories = await _categoryRepository.GetListByDynamicAsync(
                    request.DynamicQuery,
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    cancellationToken: cancellationToken);
                
                var categoriesDtos = _mapper.Map<GetListResponse<GetListCategoryByDynamicDto>>(categories);
                return categoriesDtos;

            }
        }
    }
}