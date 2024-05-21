using Application.Features.CategoryFilters.Consts;
using Application.Repositories;
using Core.Application.Rules;
using Core.CrossCuttingConcerns.Exceptions;
using Domain;

namespace Application.Features.CategoryFilters.Rules;

public class CategoryFilterBusinessRules : BaseBusinessRules
{
    private readonly ICategoryFilterRepository _categoryFilterRepository;

    public CategoryFilterBusinessRules(ICategoryFilterRepository categoryFilterRepository)
    {
        _categoryFilterRepository = categoryFilterRepository;
    }

    public Task CategoryFilterShouldExistWhenSelected(CategoryFilter? categoryFilter)
    {
        if (categoryFilter == null)
            throw new BusinessException(CategoryFiltersBusinessMessages.CategoryFilterNotExists);
        return Task.CompletedTask;
    }

    public async Task CategoryFilterIdShouldExistWhenSelected(string id, CancellationToken cancellationToken)
    {
        CategoryFilter? categoryFilter = await _categoryFilterRepository.GetAsync(
            predicate: e => e.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        await CategoryFilterShouldExistWhenSelected(categoryFilter);
    }
    
}