using Application.Features.Features.Dtos;
using Application.Repositories;
using AutoMapper;
using Core.CrossCuttingConcerns.Exceptions;
using Domain;
using MediatR;

namespace Application.Features.Features.Commands.Create;

public class CreateFeatureCommand : IRequest<CreatedFeatureResponse>
{
    public string Name { get; set; }
    public List<string>? CategoryIds { get; set; }
    
    public class CreateFeatureCommandHandler : IRequestHandler<CreateFeatureCommand, CreatedFeatureResponse>
    {
        private readonly IMapper _mapper;
        private readonly IFeatureRepository _featureRepository;
        private readonly ICategoryRepository _categoryRepository;

        public CreateFeatureCommandHandler(IMapper mapper, IFeatureRepository featureRepository, ICategoryRepository categoryRepository)
        {
            _mapper = mapper;
            _featureRepository = featureRepository;
            _categoryRepository = categoryRepository;
        }
        public async Task<CreatedFeatureResponse> Handle(CreateFeatureCommand request, CancellationToken cancellationToken)
        {
            var feature = _mapper.Map<Feature>(request);
            if (request.CategoryIds != null)
            {
                ICollection<Category> categories = new List<Category>();
                foreach (var categoryId in request.CategoryIds)
                {
                    var category = await _categoryRepository.GetAsync(c => c.Id == categoryId);
                    if (category == null)
                    {
                        throw new BusinessException("Category not found");
                    }
                    categories.Add(category);
                }
                feature.Categories = categories;
            }

            await _featureRepository.AddAsync(feature);
            CreatedFeatureResponse response = _mapper.Map<CreatedFeatureResponse>(feature);
            return response;
            
        }
        
    }
    
}