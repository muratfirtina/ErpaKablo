using Application.Features.Filters.Consts;
using Application.Repositories;
using Core.Application.Rules;
using Core.CrossCuttingConcerns.Exceptions;
using Domain;

namespace Application.Features.Filters.Rules;

public class FilterBusinessRules : BaseBusinessRules
{
    private readonly IFilterRepository _filterRepository;

    public FilterBusinessRules(IFilterRepository filterRepository)
    {
        _filterRepository = filterRepository;
    }

    public Task FilterShouldExistWhenSelected(Filter? filter)
    {
        if (filter == null)
            throw new BusinessException(FiltersBusinessMessages.FilterNotExists);
        return Task.CompletedTask;
    }

    public async Task FilterIdShouldExistWhenSelected(string id, CancellationToken cancellationToken)
    {
        Filter? filter = await _filterRepository.GetAsync(
            predicate: e => e.Id == id,
            enableTracking: false,
            cancellationToken: cancellationToken
        );
        await FilterShouldExistWhenSelected(filter);
    }
    
}