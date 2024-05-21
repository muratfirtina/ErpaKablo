using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;

namespace Application.Features.CategoryFilters.Commands.Create;

public class CreateCategoryFilterCommand : IRequest<CreatedCategoryFilterResponse>
{
    public string Name { get; set; }
    public string? CategoryId { get; set; }
    public string? FilterId { get; set; }
    
    public class CreateCategoryFilterCommandHandler : IRequestHandler<CreateCategoryFilterCommand, CreatedCategoryFilterResponse>
    {
        private readonly IMapper _mapper;
        private readonly ICategoryFilterRepository _categoryFilterRepository;

        public CreateCategoryFilterCommandHandler(IMapper mapper, ICategoryFilterRepository categoryFilterRepository)
        {
            _mapper = mapper;
            _categoryFilterRepository = categoryFilterRepository;
        }

        public async Task<CreatedCategoryFilterResponse> Handle(CreateCategoryFilterCommand request, CancellationToken cancellationToken)
        {
            var categoryFilter = _mapper.Map<CategoryFilter>(request);
            await _categoryFilterRepository.AddAsync(categoryFilter);
            
            CreatedCategoryFilterResponse response = _mapper.Map<CreatedCategoryFilterResponse>(categoryFilter);
            return response;
        }
    }
    
}