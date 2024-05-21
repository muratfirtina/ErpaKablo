using Application.Features.Filters.Rules;
using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Filters.Commands.Update;

public class UpdateFilterCommand : IRequest<UpdatedFilterResponse>
{
    public string Id { get; set; }
    public string Name { get; set; }

    public class UpdateFilterCommandHandler : IRequestHandler<UpdateFilterCommand, UpdatedFilterResponse>
    {
        private readonly IFilterRepository _filterRepository;
        private readonly FilterBusinessRules _filterBusinessRules;
        private readonly IMapper _mapper;

        public UpdateFilterCommandHandler(IFilterRepository filterRepository, IMapper mapper, FilterBusinessRules filterBusinessRules)
        {
            _filterRepository = filterRepository;
            _mapper = mapper;
            _filterBusinessRules = filterBusinessRules;
        }

        public async Task<UpdatedFilterResponse> Handle(UpdateFilterCommand request,
            CancellationToken cancellationToken)
        {
            Filter? filter = await _filterRepository.GetAsync(p => p.Id == request.Id,
                cancellationToken: cancellationToken);
            await _filterBusinessRules.FilterShouldExistWhenSelected(filter);
            if (filter != null)
            {
                filter = _mapper.Map(request, filter);
                await _filterRepository.UpdateAsync(filter);
                UpdatedFilterResponse response = _mapper.Map<UpdatedFilterResponse>(filter);
                return response;
            }
            throw new Exception("Filter not found");
        }
    }
}