using Application.Repositories;
using AutoMapper;
using Core.CrossCuttingConcerns.Exceptions;
using Domain;
using MediatR;

namespace Application.Features.Categories.Commands.Create;

public class CreateCategoryCommand : IRequest<CreatedCategoryResponse>
{
    public string Name { get; set; }
    public string? ParentCategoryId { get; set; }
    
    public class CreateCategoryCommandHandler : IRequestHandler<CreateCategoryCommand, CreatedCategoryResponse>
    {
        private readonly IMapper _mapper;
        private readonly ICategoryRepository _categoryRepository;

        public CreateCategoryCommandHandler(IMapper mapper, ICategoryRepository categoryRepository)
        {
            _mapper = mapper;
            _categoryRepository = categoryRepository;
        }

        public async Task<CreatedCategoryResponse> Handle(CreateCategoryCommand request, CancellationToken cancellationToken)
        {
            
            
            //Böyle bir kategori ismi var mı bak.
            var category = await _categoryRepository.GetAsync(c => c.Name == request.Name, cancellationToken: cancellationToken);
            if (category != null)
            {
                throw new BusinessException("Bu isimde bir kategori zaten var.");
            }
            //parentCategoryId yoksa null yap
            if (string.IsNullOrEmpty(request.ParentCategoryId))
            {
                request.ParentCategoryId = null;
            }

            Category mappedCategory = _mapper.Map<Category>(request);
            await _categoryRepository.AddAsync(mappedCategory);
            CreatedCategoryResponse response = _mapper.Map<CreatedCategoryResponse>(mappedCategory);
            return response;
        }
    }
    
}