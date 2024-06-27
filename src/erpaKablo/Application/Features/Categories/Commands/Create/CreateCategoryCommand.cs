using Application.Features.Categories.Dtos;
using Application.Features.Categories.Rules;
using Application.Repositories;
using AutoMapper;
using Core.CrossCuttingConcerns.Exceptions;
using Domain;
using MediatR;

namespace Application.Features.Categories.Commands.Create;

public class CreateCategoryCommand : IRequest<CreatedCategoryResponse>
{
    public string Name { get; set; }
    public string? ParentCategoryId { get; set; }
    public List<string>? FeatureIds { get; set; }
    
    
    
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CreatedCategoryResponse>
    {
        private readonly IMapper _mapper;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IFeatureRepository _featureRepository;
        private readonly CategoryBusinessRules _categoryBusinessRules;

        public CreateCategoryCommandHandler(IMapper mapper, ICategoryRepository categoryRepository, IFeatureRepository featureRepository, CategoryBusinessRules categoryBusinessRules)
        {
            _mapper = mapper;
            _categoryRepository = categoryRepository;
            _featureRepository = featureRepository;
            _categoryBusinessRules = categoryBusinessRules;
        }

        public async Task<CreatedCategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            
            await _categoryBusinessRules.CategoryNameShouldBeUniqueWhenCreate(request.Name, cancellationToken);
    
            var category = _mapper.Map<Category>(request);

            if (request.ParentCategoryId != null)
            {
                await _categoryBusinessRules.CategoryIdShouldExistWhenSelected(request.ParentCategoryId, cancellationToken);
                var parentCategory = await _categoryRepository.GetAsync(category => category.Id == request.ParentCategoryId);
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
                        throw new BusinessException("Feature not found");
                    }
                    features.Add(feature);
                }
                category.Features = features;
            }

            await _categoryRepository.AddAsync(category);
            CreatedCategoryResponse response = _mapper.Map<CreatedCategoryResponse>(category);
            return response;
        }

    }
    
}