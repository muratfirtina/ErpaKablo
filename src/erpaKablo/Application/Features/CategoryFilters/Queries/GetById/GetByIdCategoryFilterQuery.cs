using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CategoryFilters.Queries.GetById;

public class GetByIdCategoryFilterQuery : IRequest<GetByIdCategoryFilterResponse>
{
    public string Id { get; set; }
    
    public class GetByIdCategoryFilterQueryHandler : IRequestHandler<GetByIdCategoryFilterQuery, GetByIdCategoryFilterResponse>
    {
        private readonly ICategoryFilterRepository _categoryFilterRepository;
        private readonly IMapper _mapper;

        public GetByIdCategoryFilterQueryHandler(ICategoryFilterRepository categoryFilterRepository, IMapper mapper)
        {
            _categoryFilterRepository = categoryFilterRepository;
            _mapper = mapper;
        }

        public async Task<GetByIdCategoryFilterResponse> Handle(GetByIdCategoryFilterQuery request, CancellationToken cancellationToken)
        {
            CategoryFilter? categoryFilter = await _categoryFilterRepository.GetAsync(
                predicate: p => p.Id == request.Id,
                cancellationToken: cancellationToken);
            GetByIdCategoryFilterResponse response = _mapper.Map<GetByIdCategoryFilterResponse>(categoryFilter);
            return response;
        }
    }
}