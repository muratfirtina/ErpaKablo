using Application.Features.Categories.Rules;
using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Commands.Update;

public class UpdateCategoryCommand : IRequest<UpdatedCategoryResponse>
{
    public string Id { get; set; }
    public string? Name { get; set; }
    public string? ParentCategoryId { get; set; }
    public List<string>? SubCategoryIds { get; set; }
    public List<string>? FeatureIds { get; set; }

    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, UpdatedCategoryResponse>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly CategoryBusinessRules _categoryBusinessRules;
        private readonly IFeatureRepository _featureRepository;
        private readonly IMapper _mapper;

        public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IMapper mapper,
            CategoryBusinessRules categoryBusinessRules, IFeatureRepository featureRepository)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _categoryBusinessRules = categoryBusinessRules;
            _featureRepository = featureRepository;
        }

        public async Task<UpdatedCategoryResponse> Handle(UpdateCategoryCommand request,
            CancellationToken cancellationToken)
        {
            Category? category = await _categoryRepository.GetAsync(p => p.Id == request.Id,
                include: c => c.Include(c => c.Features), cancellationToken: cancellationToken);
            await _categoryBusinessRules.CategoryShouldExistWhenSelected(category);

            await _categoryBusinessRules.CategoryNameShouldBeUniqueWhenUpdate(request.Name, request.Id,
                cancellationToken);
            await _categoryBusinessRules.ParentCategoryShouldNotBeSelf(request.Id, request.ParentCategoryId,
                cancellationToken);
            await _categoryBusinessRules.ParentCategoryShouldNotBeChild(request.Id, request.ParentCategoryId,
                cancellationToken);

            if (request.ParentCategoryId != null)
            {
                await _categoryBusinessRules.ParentCategoryShouldNotBeDescendant(request.Id, request.ParentCategoryId,
                    cancellationToken);
                var parentCategory = await _categoryRepository.GetAsync(c => c.Id == request.ParentCategoryId);
                if (parentCategory == null)
                {
                    throw new Exception("Parent category not found");
                }

                category.ParentCategory = parentCategory;
                category.ParentCategoryId = request.ParentCategoryId;
            }
            else
            {
                // ParentCategoryId null olduğunda, kategoriyi en üst seviye kategori yap
                category.ParentCategory = null;
                category.ParentCategoryId = null;
            }

            if (request.FeatureIds != null)
            {
                category.Features.Clear(); // Mevcut özellikleri temizle
                foreach (var featureId in request.FeatureIds)
                {
                    var feature = await _featureRepository.GetAsync(feature => feature.Id == featureId);
                    if (feature == null)
                    {
                        throw new Exception($"Feature with id {featureId} not found");
                    }

                    category.Features.Add(feature);
                }
            }
            else
            {
                category.Features.Clear(); // FeatureIds null ise tüm özellikleri kaldır
            }

            category.Name = request.Name ?? category.Name;

            await _categoryRepository.UpdateAsync(category);

            UpdatedCategoryResponse response = _mapper.Map<UpdatedCategoryResponse>(category);
            return response;
        }
    }
}