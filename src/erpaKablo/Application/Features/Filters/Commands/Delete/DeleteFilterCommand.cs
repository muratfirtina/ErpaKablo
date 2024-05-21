using Application.Features.Filters.Rules;
using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;

namespace Application.Features.Filters.Commands.Delete;

public class DeleteFilterCommand : IRequest<DeletedFilterResponse>
{
    public string Id { get; set; }
    
    public class DeleteFilterCommandHandler : IRequestHandler<DeleteFilterCommand, DeletedFilterResponse>
    {
        private readonly IFilterRepository _filterRepository;
        private readonly FilterBusinessRules _filterBusinessRules;
        private readonly IMapper _mapper;

        public DeleteFilterCommandHandler(IFilterRepository filterRepository, IMapper mapper, FilterBusinessRules filterBusinessRules)
        {
            _filterRepository = filterRepository;
            _mapper = mapper;
            _filterBusinessRules = filterBusinessRules;
        }

        public async Task<DeletedFilterResponse> Handle(DeleteFilterCommand request, CancellationToken cancellationToken)
        {
            Filter? filter = await _filterRepository.GetAsync(p=>p.Id==request.Id,cancellationToken: cancellationToken);
            await _filterBusinessRules.FilterShouldExistWhenSelected(filter);
            await _filterRepository.DeleteAsync(filter!);
            DeletedFilterResponse response = _mapper.Map<DeletedFilterResponse>(filter);
            return response;
        }
    }
}