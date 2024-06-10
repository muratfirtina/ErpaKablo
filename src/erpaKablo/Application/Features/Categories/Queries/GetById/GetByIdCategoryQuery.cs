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
        private readonly IProductVariantRepository _productVariantRepository;
        private readonly IMapper _mapper;

        public GetByIdCategoryQueryHandler(ICategoryRepository categoryRepository, IMapper mapper, IProductVariantRepository productVariantRepository)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _productVariantRepository = productVariantRepository;
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
                    .Include(fv=>fv.Products)
                    .ThenInclude(p=>p.ProductVariants)
                    .ThenInclude(pv=>pv.VariantFeatureValues),
                cancellationToken: cancellationToken);

            if (category == null)
            {
                throw new BusinessException("Category not found.");
            }

            // Özellik ve değer bazlı ürün varyantı sayısını hesaplayın
            var featureValueProductCounts = new Dictionary<string, Dictionary<string, int>>();

            foreach (var feature in category.Features)
            {
                var valueCounts = new Dictionary<string, int>();

                foreach (var featureValue in feature.FeatureValues)
                {
                    int count = category.Products
                        .SelectMany(p => p.ProductVariants)
                        .Count(pv => pv.VariantFeatureValues.Any(vfv => vfv.FeatureId == feature.Id && vfv.FeatureValueId == featureValue.Id));

                    valueCounts[featureValue.Value] = count;
                }

                featureValueProductCounts[feature.Name] = valueCounts;
            }
            
            GetByIdCategoryResponse response = _mapper.Map<GetByIdCategoryResponse>(category);
            response.FeatureValueProductCounts = featureValueProductCounts;

            return response;
        }

    }
}