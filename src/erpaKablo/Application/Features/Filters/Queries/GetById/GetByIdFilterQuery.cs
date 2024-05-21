using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Filters.Queries.GetById;

public class GetByIdFilterQuery : IRequest<GetByIdFilterResponse>
{
    public string Id { get; set; }
    
    public class GetByIdFilterQueryHandler : IRequestHandler<GetByIdFilterQuery, GetByIdFilterResponse>
    {
        private readonly IFilterRepository _filterRepository;
        private readonly IMapper _mapper;

        public GetByIdFilterQueryHandler(IFilterRepository filterRepository, IMapper mapper)
        {
            _filterRepository = filterRepository;
            _mapper = mapper;
        }

        public async Task<GetByIdFilterResponse> Handle(GetByIdFilterQuery request, CancellationToken cancellationToken)
        {
            Filter? filter = await _filterRepository.GetAsync(
                predicate: p => p.Id == request.Id,
                cancellationToken: cancellationToken);
            GetByIdFilterResponse response = _mapper.Map<GetByIdFilterResponse>(filter);
            return response;
        }
    }
}