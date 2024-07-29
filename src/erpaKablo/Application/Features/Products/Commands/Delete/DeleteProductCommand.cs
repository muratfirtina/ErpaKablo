using Application.Features.Products.Rules;
using Application.Repositories;
using AutoMapper;
using Domain;
using MediatR;

namespace Application.Features.Products.Commands.Delete;

public class DeleteProductCommand : IRequest<DeletedProductResponse>
{
    public string Id { get; set; }
    
    public class DeleteProductCommandHandler : IRequestHandler<DeleteProductCommand, DeletedProductResponse>
    {
        private readonly IProductRepository _productRepository;
        private readonly ProductBusinessRules _productBusinessRules;
        private readonly IMapper _mapper;

        public DeleteProductCommandHandler(IProductRepository productRepository, IMapper mapper, ProductBusinessRules productBusinessRules)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _productBusinessRules = productBusinessRules;
        }

        public async Task<DeletedProductResponse> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            Product? product = await _productRepository.GetAsync(p=>p.Id==request.Id,cancellationToken: cancellationToken);
            await _productBusinessRules.ProductShouldExistWhenSelected(product);
            await _productRepository.DeleteAsync(product!);
            DeletedProductResponse response = _mapper.Map<DeletedProductResponse>(product);
            response.Success = true;
            return response;
        }
    }
}