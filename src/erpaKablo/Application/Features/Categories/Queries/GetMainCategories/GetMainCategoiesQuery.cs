using Application.Features.Categories.Dtos;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Queries.GetMainCategories;

public class GetMainCategoiesQuery : IRequest<GetListResponse<GetMainCategoriesResponse>>
{
    public PageRequest PageRequest { get; set; }

    public class
        GetMainCategoriesQueryHandler : IRequestHandler<GetMainCategoiesQuery,
        GetListResponse<GetMainCategoriesResponse>>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;

        public GetMainCategoriesQueryHandler(ICategoryRepository categoryRepository, IMapper mapper,
            IStorageService storageService)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<GetListResponse<GetMainCategoriesResponse>> Handle(GetMainCategoiesQuery request,
            CancellationToken cancellationToken)
        {
            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                List<Category> categories = await _categoryRepository.GetAllAsync(
                    predicate: x => x.ParentCategoryId == null,
                    include: x => x.Include(x => x.CategoryImageFiles),
                    cancellationToken: cancellationToken);

                GetListResponse<GetMainCategoriesResponse> response =
                    _mapper.Map<GetListResponse<GetMainCategoriesResponse>>(categories);
                SetCategoryImageUrls(response.Items);
                return response;
            }
            else
            {
                IPaginate<Category> categories = await _categoryRepository.GetListAsync(
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    predicate: x => x.ParentCategoryId == null,
                    include: x => x.Include(x => x.CategoryImageFiles),
                    cancellationToken: cancellationToken);

                GetListResponse<GetMainCategoriesResponse> response =
                    _mapper.Map<GetListResponse<GetMainCategoriesResponse>>(categories);
                SetCategoryImageUrls(response.Items);
                return response;
            }
        }
        
        private void SetCategoryImageUrls(IEnumerable<GetMainCategoriesResponse> categories)
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