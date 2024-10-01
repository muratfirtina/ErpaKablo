using Application.Extensions;
using Application.Features.Categories.Dtos;
using Application.Features.Categories.Rules;
using Application.Features.Products.Dtos;
using Application.Repositories;
using Application.Storage;
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

    public class GetListByDynamicCategoryQueryHandler : IRequestHandler<GetListCategoryByDynamicQuery,
        GetListResponse<GetListCategoryByDynamicDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly CategoryBusinessRules _categoryBusinessRules;
        private readonly IStorageService _storageService;

        public GetListByDynamicCategoryQueryHandler(ICategoryRepository categoryRepository, IMapper mapper,
            CategoryBusinessRules categoryBusinessRules, IStorageService storageService)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _categoryBusinessRules = categoryBusinessRules;
            _storageService = storageService;
        }

        public async Task<GetListResponse<GetListCategoryByDynamicDto>> Handle(GetListCategoryByDynamicQuery request,
            CancellationToken cancellationToken)
        {
            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                var allCategories = await _categoryRepository.GetAllByDynamicAsync(
                    request.DynamicQuery,
                    include: q => q
                        .Include(c => c.Products)
                        .ThenInclude(p => p.ProductImageFiles.Where(pif => pif.Showcase))
                        .Include(c => c.SubCategories).ThenInclude(sc => sc.Products)
                        .Include(c => c.CategoryImageFiles),
                    cancellationToken: cancellationToken);

                var categoriesDtos = _mapper.Map<GetListResponse<GetListCategoryByDynamicDto>>(allCategories);
                
                categoriesDtos.Items.SetImageUrl(_storageService);

                foreach (var category in categoriesDtos.Items)
                {
                    category.SubCategories = await GetSubCategoriesRecursively(category.Id, cancellationToken);
                    category.Products = _mapper.Map<List<ProductDto>>(allCategories.FirstOrDefault(c => c.Id == category.Id)?.Products);
                    
                    var categoryImageFile = allCategories.FirstOrDefault(c => c.Id == category.Id)?.CategoryImageFiles?.FirstOrDefault();
                    if (categoryImageFile != null)
                    {
                        category.CategoryImage = _mapper.Map<CategoryImageFileDto>(categoryImageFile);
                    }
                }
                
                return new GetListResponse<GetListCategoryByDynamicDto>
                {
                    Items = categoriesDtos.Items,
                    Index = 0,
                    Size = categoriesDtos.Count,
                    Count = categoriesDtos.Count,
                    Pages = 1,
                    HasNext = false,
                    HasPrevious = false
                };
            }
            else
            {
                IPaginate<Category> categories = await _categoryRepository.GetListByDynamicAsync(
                    request.DynamicQuery,
                    include: q => q
                        .Include(c => c.Products)
                        .ThenInclude(p => p.ProductImageFiles.Where(pif => pif.Showcase))
                        .Include(c => c.SubCategories).ThenInclude(sc => sc.Products)
                        .Include(c => c.CategoryImageFiles),
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    cancellationToken: cancellationToken);

                var categoriesDtos = _mapper.Map<GetListResponse<GetListCategoryByDynamicDto>>(categories);
                
                categoriesDtos.Items.SetImageUrl(_storageService);

                foreach (var category in categoriesDtos.Items)
                {
                    category.SubCategories = await GetSubCategoriesRecursively(category.Id, cancellationToken);
                    category.Products = _mapper.Map<List<ProductDto>>(categories.Items.FirstOrDefault(c => c.Id == category.Id)?.Products);
                    
                    var categoryImageFile = categories.Items.FirstOrDefault(c => c.Id == category.Id)?.CategoryImageFiles?.FirstOrDefault();
                    if (categoryImageFile != null)
                    {
                        category.CategoryImage = _mapper.Map<CategoryImageFileDto>(categoryImageFile);
                    }
                }
                

                return new GetListResponse<GetListCategoryByDynamicDto>
                {
                    Items = categoriesDtos.Items,
                    Index = categories.Index,
                    Size = categories.Size,
                    Count = categories.Count,
                    Pages = categories.Pages,
                    HasNext = categories.HasNext,
                    HasPrevious = categories.HasPrevious
                };
            }
        }

        private async Task<List<GetListCategoryByDynamicDto>> GetSubCategoriesRecursively(string parentId, CancellationToken cancellationToken)
        {
            var subCategories = await _categoryRepository.GetListAsync(
                predicate: c => c.ParentCategoryId == parentId,
                include: q => q
                    .Include(c => c.Products)
                    .ThenInclude(p => p.ProductImageFiles.Where(pif => pif.Showcase))
                    .Include(c => c.SubCategories).ThenInclude(sc => sc.Products)
                    .Include(c => c.CategoryImageFiles),
                cancellationToken: cancellationToken
            );

            var subCategoryDtos = _mapper.Map<List<GetListCategoryByDynamicDto>>(subCategories.Items);

            foreach (var subCategory in subCategoryDtos)
            {
                subCategory.SubCategories = await GetSubCategoriesRecursively(subCategory.Id, cancellationToken);
                subCategory.Products = _mapper.Map<List<ProductDto>>(subCategories.Items.FirstOrDefault(c => c.Id == subCategory.Id)?.Products);
                
                var categoryImageFile = subCategories.Items.FirstOrDefault(c => c.Id == subCategory.Id)?.CategoryImageFiles?.FirstOrDefault();
                if (categoryImageFile != null)
                {
                    subCategory.CategoryImage = _mapper.Map<CategoryImageFileDto>(categoryImageFile);
                }
            }

            return subCategoryDtos;
        }
        
    }
}