using Application.Features.Categories.Dtos;
using Application.Features.Categories.Queries.GetList;
using Application.Repositories;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Application.Features.Categories.Queries.GetList
{
    public class GetAllCategoryQuery : IRequest<GetListResponse<GetAllCategoryQueryResponse>>
    {
        public PageRequest PageRequest { get; set; }
        
        public class GetAllCategoryQueryHandler : IRequestHandler<GetAllCategoryQuery, GetListResponse<GetAllCategoryQueryResponse>>
        {
            private readonly ICategoryRepository _categoryRepository;
            private readonly IMapper _mapper;

            public GetAllCategoryQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
            {
                _categoryRepository = categoryRepository;
                _mapper = mapper;
            }

            public async Task<GetListResponse<GetAllCategoryQueryResponse>> Handle(GetAllCategoryQuery request, CancellationToken cancellationToken)
            {
                if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
                {
                    List<Category> categories = await _categoryRepository.GetAllAsync(cancellationToken: cancellationToken);
                    
                    GetListResponse<GetAllCategoryQueryResponse> response = _mapper.Map<GetListResponse<GetAllCategoryQueryResponse>>(categories);
                    return response;
                }
                else
                {
                    IPaginate<Category> categories = await _categoryRepository.GetListAsync(
                        index: request.PageRequest.PageIndex,
                        size: request.PageRequest.PageSize,
                        cancellationToken: cancellationToken
                    );
                    
                    GetListResponse<GetAllCategoryQueryResponse> response = _mapper.Map<GetListResponse<GetAllCategoryQueryResponse>>(categories);
                    return response;
                }
            }

            
        }
    }
}