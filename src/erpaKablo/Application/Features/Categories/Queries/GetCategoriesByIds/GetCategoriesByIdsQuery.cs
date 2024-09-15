using Application.Features.Categories.Dtos;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Queries.GetCategoriesByIds;

public class GetCategoriesByIdsQuery : IRequest<GetListResponse<GetCategoriesByIdsQueryResponse>>
{
    public List<string> Ids { get; set; }
    
    public class GetCategoriesByIdsQueryHandler : IRequestHandler<GetCategoriesByIdsQuery, GetListResponse<GetCategoriesByIdsQueryResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;

        public GetCategoriesByIdsQueryHandler(ICategoryRepository categoryRepository, IMapper mapper, IStorageService storageService)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<GetListResponse<GetCategoriesByIdsQueryResponse>> Handle(GetCategoriesByIdsQuery request, CancellationToken cancellationToken)
        {
            List<Category> categories = await _categoryRepository.GetAllAsync(
                index:-1,
                size:-1,
                predicate: x => request.Ids.Contains(x.Id),
                include: c => c.Include(c => c.ParentCategory)
                    .Include(c => c.CategoryImageFiles)
                    .Include(c => c.SubCategories)
                    .Include(c => c.Features)
                    .ThenInclude(f => f.FeatureValues)
                    .Include(fv => fv.Products),
                cancellationToken: cancellationToken
            );

            GetListResponse<GetCategoriesByIdsQueryResponse> response = _mapper.Map<GetListResponse<GetCategoriesByIdsQueryResponse>>(categories);
            SetCategoryImageUrls(response.Items);
            return response;
        }
        
        private void SetCategoryImageUrls(IEnumerable<GetCategoriesByIdsQueryResponse> categories)
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
                        Path = "default",
                        FileName = "default.png",
                        Url = $"{baseUrl}categories/default/default.png"
                    };
                }
            }
        }
    }
}