using Application.Features.Categories.Rules;
using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Categories.Commands.Update;

public class UpdateCategoryCommand : IRequest<UpdatedCategoryResponse>
{
    public string Id { get; set; }
    public string Name { get; set; }

    public class UpdateCategoryCommandHandler : IRequestHandler<UpdateCategoryCommand, UpdatedCategoryResponse>
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly CategoryBusinessRules _categoryBusinessRules;
        private readonly IMapper _mapper;

        public UpdateCategoryCommandHandler(ICategoryRepository categoryRepository, IMapper mapper, CategoryBusinessRules categoryBusinessRules)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
            _categoryBusinessRules = categoryBusinessRules;
        }

        public async Task<UpdatedCategoryResponse> Handle(UpdateCategoryCommand request,
            CancellationToken cancellationToken)
        {
            Category? category = await _categoryRepository.GetAsync(p => p.Id == request.Id,
                cancellationToken: cancellationToken);
            await _categoryBusinessRules.CategoryShouldExistWhenSelected(category);
            if (category != null)
            {
                category = _mapper.Map(request, category);
                await _categoryRepository.UpdateAsync(category);
                UpdatedCategoryResponse response = _mapper.Map<UpdatedCategoryResponse>(category);
                return response;
            }
            throw new Exception("Category not found");
        }
    }
}