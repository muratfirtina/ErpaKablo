using Application.Features.Products.Dtos.FilterDto;
using Application.Repositories;
using AutoMapper;
using MediatR;

namespace Application.Features.Products.Queries.SearchAndFilter.Filter.GetAvailableFilter;

public class GetAvailableFiltersQuery : IRequest<List<FilterGroupDto>> { }

public class GetAvailableFiltersQueryHandler : IRequestHandler<GetAvailableFiltersQuery, List<FilterGroupDto>>
{
    private readonly IProductRepository _productRepository;
    private readonly IMapper _mapper;

    public GetAvailableFiltersQueryHandler(IProductRepository productRepository, IMapper mapper)
    {
        _productRepository = productRepository;
        _mapper = mapper;
    }

    public async Task<List<FilterGroupDto>> Handle(GetAvailableFiltersQuery request, CancellationToken cancellationToken)
    {
        var filters = await _productRepository.GetAvailableFilters();
        return _mapper.Map<List<FilterGroupDto>>(filters);
    }
}