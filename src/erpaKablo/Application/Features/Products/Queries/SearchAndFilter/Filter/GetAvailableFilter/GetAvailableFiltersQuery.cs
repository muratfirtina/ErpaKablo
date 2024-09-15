using Application.Features.Products.Dtos.FilterDto;
using Application.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Products.Queries.SearchAndFilter.Filter.GetAvailableFilter;

public class GetAvailableFiltersQuery : IRequest<List<FilterDefinitionDto>> { }

public class GetAvailableFiltersQueryHandler : IRequestHandler<GetAvailableFiltersQuery, List<FilterDefinitionDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetAvailableFiltersQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<List<FilterDefinitionDto>> Handle(GetAvailableFiltersQuery request, CancellationToken cancellationToken)
    {
        var filters = await _productRepository.GetAvailableFilters();
        return _mapper.Map<List<FilterDefinitionDto>>(filters);
    }
}