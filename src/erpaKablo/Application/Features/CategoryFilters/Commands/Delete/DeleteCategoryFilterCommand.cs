using Application.Features.CategoryFilters.Rules;
using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;

namespace Application.Features.CategoryFilters.Commands.Delete;

public class DeleteCategoryFilterCommand : IRequest<DeletedCategoryFilterResponse>
{
    public string Id { get; set; }
    
    public class DeleteCategoryFilterCommandHandler : IRequestHandler<DeleteCategoryFilterCommand, DeletedCategoryFilterResponse>
    {
        private readonly ICategoryFilterRepository _categoryFilterRepository;
        private readonly CategoryFilterBusinessRules _categoryFilterBusinessRules;
        private readonly IMapper _mapper;

        public DeleteCategoryFilterCommandHandler(ICategoryFilterRepository categoryFilterRepository, IMapper mapper, CategoryFilterBusinessRules categoryFilterBusinessRules)
        {
            _categoryFilterRepository = categoryFilterRepository;
            _mapper = mapper;
            _categoryFilterBusinessRules = categoryFilterBusinessRules;
        }

        public async Task<DeletedCategoryFilterResponse> Handle(DeleteCategoryFilterCommand request, CancellationToken cancellationToken)
        {
            CategoryFilter? categoryFilter = await _categoryFilterRepository.GetAsync(p=>p.Id==request.Id,cancellationToken: cancellationToken);
            await _categoryFilterBusinessRules.CategoryFilterShouldExistWhenSelected(categoryFilter);
            await _categoryFilterRepository.DeleteAsync(categoryFilter!);
            DeletedCategoryFilterResponse response = _mapper.Map<DeletedCategoryFilterResponse>(categoryFilter);
            return response;
        }
    }
}