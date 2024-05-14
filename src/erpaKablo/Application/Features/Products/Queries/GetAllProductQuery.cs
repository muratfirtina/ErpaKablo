using Application.Features.Products.Dtos;
using Application.Repositories;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries;

public class GetAllProductQuery : IRequest<GetListResponse<GetAllProductQueryResponse>>
{
    public PageRequest PageRequest { get; set; }
    
    public class GetAllProductQueryHandler : IRequestHandler<GetAllProductQuery, GetListResponse<GetAllProductQueryResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IMapper _mapper;

        public GetAllProductQueryHandler(IProductRepository productRepository, IMapper mapper)
        {
            _productRepository = productRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetAllProductQueryResponse>> Handle(GetAllProductQuery request, CancellationToken cancellationToken)
        {
            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                List<Product> products = await _productRepository.GetAllAsync(
                    include: x => 
                        x.Include(x => x.Category)
                            .Include(x => x.Brand)
                            .Include(x => x.ProductFeatures).ThenInclude(x => x.Features),
                    cancellationToken: cancellationToken);
                GetListResponse<GetAllProductQueryResponse> response = _mapper.Map<GetListResponse<GetAllProductQueryResponse>>(products);
                return response;
            }
            else
            {
                IPaginate<Product> products = await _productRepository.GetListAsync(
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    include: x => x.Include(x => x.Category).Include(x => x.Brand),
                    cancellationToken: cancellationToken
                );
                GetListResponse<GetAllProductQueryResponse> response = _mapper.Map<GetListResponse<GetAllProductQueryResponse>>(products);
                return response;
            }
        }
    }
}