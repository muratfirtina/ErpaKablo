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
    public List<string>? SubCategoryIds{ get; set; }
    public List<string>? FeatureIds { get; set; }

    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, UpdatedCategoryResponse>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly CategoryBusinessRules _categoryBusinessRules;
        private readonly IFeatureRepository _featureRepository;
        private readonly IMapper _mapper;

        public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IMapper mapper, CategoryBusinessRules categoryBusinessRules, IFeatureRepository featureRepository)
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
                cancellationToken: cancellationToken);
            await _categoryBusinessRules.CategoryShouldExistWhenSelected(category);
            
            if (request.ParentCategoryId != null)
            {
                var parentCategory = await _categoryRepository.GetAsync(category => category.Id == request.ParentCategoryId);
                if (parentCategory == null)
                {
                    throw new Exception("Parent category not found");
                }
                category.ParentCategory = parentCategory;
            }
            
            if (request.FeatureIds != null)
            {
                ICollection<Feature> features = new List<Feature>();
                foreach (var featureId in request.FeatureIds)
                {
                    var feature = await _featureRepository.GetAsync(feature => feature.Id == featureId);
                    if (feature == null)
                    {
                        throw new Exception("Feature not found");
                    }
                    features.Add(feature);
                }
                category.Features = features;
            }
            
            if (category != null)
            {
                category = _mapper.Map(request, category);
                await _categoryRepository.UpdateAsync(category);
                UpdatedCategoryResponse response = _mapper.Map<UpdatedCategoryResponse>(category);
                return response;
            }
            throw new Exception("Category not found");
        }
    }
}