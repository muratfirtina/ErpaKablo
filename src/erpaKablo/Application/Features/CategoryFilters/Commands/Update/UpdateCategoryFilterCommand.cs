using Application.Features.CategoryFilters.Rules;
using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.CategoryFilters.Commands.Update;

public class UpdateCategoryFilterCommand : IRequest<UpdatedCategoryFilterResponse>
{
    public string Id { get; set; }
    public string Name { get; set; }

    public class UpdateCategoryFilterCommandHandler : IRequestHandler<UpdateCategoryFilterCommand, UpdatedCategoryFilterResponse>
    {
        private readonly ICategoryFilterRepository _categoryFilterRepository;
        private readonly CategoryFilterBusinessRules _categoryFilterBusinessRules;
        private readonly IMapper _mapper;

        public UpdateCategoryFilterCommandHandler(ICategoryFilterRepository categoryFilterRepository, IMapper mapper, CategoryFilterBusinessRules categoryFilterBusinessRules)
        {
            _categoryFilterRepository = categoryFilterRepository;
            _mapper = mapper;
            _categoryFilterBusinessRules = categoryFilterBusinessRules;
        }

        public async Task<UpdatedCategoryFilterResponse> Handle(UpdateCategoryFilterCommand request,
            CancellationToken cancellationToken)
        {
            CategoryFilter? categoryFilter = await _categoryFilterRepository.GetAsync(p => p.Id == request.Id,
                cancellationToken: cancellationToken);
            await _categoryFilterBusinessRules.CategoryFilterShouldExistWhenSelected(categoryFilter);
            if (categoryFilter != null)
            {
                categoryFilter = _mapper.Map(request, categoryFilter);
                await _categoryFilterRepository.UpdateAsync(categoryFilter);
                UpdatedCategoryFilterResponse response = _mapper.Map<UpdatedCategoryFilterResponse>(categoryFilter);
                return response;
            }
            throw new Exception("CategoryFilter not found");
        }
    }
}