using Application.Features.Categories.Rules;
using Application.Features.Products.Dtos;
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

    public class GetListByDynamicCategoryQueryHandler : IRequestHandler<GetListCategoryByDynamicQuery,
        GetListResponse<GetListCategoryByDynamicDto>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly CategoryBusinessRules _categoryBusinessRules;

        public GetListByDynamicCategoryQueryHandler(ICategoryRepository categoryRepository, IMapper mapper,
            CategoryBusinessRules categoryBusinessRules)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _categoryBusinessRules = categoryBusinessRules;
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
                        .Include(c => c.SubCategories).ThenInclude(sc => sc.Products),
                    cancellationToken: cancellationToken);

                var categoriesDtos = _mapper.Map<GetListResponse<GetListCategoryByDynamicDto>>(allCategories);

                foreach (var category in categoriesDtos.Items)
                {
                    category.SubCategories = await GetSubCategoriesRecursively(category.Id, cancellationToken);
                    // Ana kategorilerin ürünlerini de mapliyoruz
                    category.Products = _mapper.Map<List<ProductDto>>(categoriesDtos.Items.First(c => c.Id == category.Id).Products);
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
                        .Include(c => c.SubCategories).ThenInclude(sc => sc.Products),
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    cancellationToken: cancellationToken);

                var categoriesDtos = _mapper.Map<GetListResponse<GetListCategoryByDynamicDto>>(categories);

                foreach (var category in categoriesDtos.Items)
                {
                    category.SubCategories = await GetSubCategoriesRecursively(category.Id, cancellationToken);
                    // Ana kategorilerin ürünlerini de mapliyoruz
                    category.Products = _mapper.Map<List<ProductDto>>(categoriesDtos.Items.First(c => c.Id == category.Id).Products);
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
                    .Include(c => c.SubCategories).ThenInclude(sc => sc.Products),
                cancellationToken: cancellationToken
            );

            var subCategoryDtos = _mapper.Map<List<GetListCategoryByDynamicDto>>(subCategories.Items);

            foreach (var subCategory in subCategoryDtos)
            {
                subCategory.SubCategories = await GetSubCategoriesRecursively(subCategory.Id, cancellationToken);
                subCategory.Products = _mapper.Map<List<ProductDto>>(subCategories.Items.First(c => c.Id == subCategory.Id).Products);
            }

            return subCategoryDtos;
        }
    }
}
