using Application.Features.Categories.Dtos;
using Application.Repositories;
using Application.Storage;
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
        private readonly IStorageService _storageService;

        public GetByIdCategoryQueryHandler(ICategoryRepository categoryRepository, IMapper mapper, IStorageService storageService)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<GetByIdCategoryResponse> Handle(GetByIdCategoryQuery request, CancellationToken cancellationToken)
        {
            // Kategoriyi ve ilgili verileri alın
            Category? category = await _categoryRepository.GetAsync(
                predicate: p => p.Id == request.Id,
                include: c => c.Include(c => c.ParentCategory)
                    .Include(c => c.CategoryImageFiles)
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
            SetCategoryImageUrls(new List<GetByIdCategoryResponse> {response});
            return response;
        }
        
        private void SetCategoryImageUrls(IEnumerable<GetByIdCategoryResponse> categories)
        {
            var baseUrl = _storageService.GetStorageUrl();
            foreach (var category in categories)
            {
                if (category.CategoryImage != null)
                {
                    category.CategoryImage.Url = $"{baseUrl}{category.CategoryImage.EntityType}/{category.CategoryImage.Path}/{category.CategoryImage.FileName}";
                }
                else
                {
                    category.CategoryImage = new CategoryImageFileDto
                    {
                        EntityType = "categories",
                        Path = "",
                        FileName = "default-category-image.png",
                        Url = $"{baseUrl}categories/default-category-image.png"
                    };
                }
            }
        }

    }
}