using Application.Repositories;
using AutoMapper;
using Core.CrossCuttingConcerns.Exceptions;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Queries.GetById;

public class GetByIdCategoryQuery : IRequest<GetByIdCategoryResponse>
{
    public string Id { get; set; }
    
    public class GetByIdCategoryQueryHandler : IRequestHandler<GetByIdCategoryQuery, GetByIdCategoryResponse>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public GetByIdCategoryQueryHandler(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<GetByIdCategoryResponse> Handle(GetByIdCategoryQuery request, CancellationToken cancellationToken)
        {
            // Kategoriyi ve ilgili verileri alın
            Category? category = await _categoryRepository.GetAsync(
                predicate: p => p.Id == request.Id,
                include: c => c.Include(c => c.ParentCategory)
                    .Include(c => c.SubCategories)
                    .Include(c => c.Features)
                    .ThenInclude(f => f.FeatureValues)
                    .Include(fv=>fv.Products),
                cancellationToken: cancellationToken);

            if (category == null)
            {
                throw new BusinessException("Category not found.");
            }
            
            GetByIdCategoryResponse response = _mapper.Map<GetByIdCategoryResponse>(category);
            return response;
        }

    }
}