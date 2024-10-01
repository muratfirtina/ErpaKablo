using Application.Extensions;
using Application.Features.ProductImageFiles.Dtos;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Products.Queries.GetList;

public class GetAllProductQuery : IRequest<GetListResponse<GetAllProductQueryResponse>>
{
    public PageRequest PageRequest { get; set; }

    public class
        GetAllProductQueryHandler : IRequestHandler<GetAllProductQuery, GetListResponse<GetAllProductQueryResponse>>
    {
        private readonly IProductRepository _productRepository;
        private readonly IProductLikeRepository _productLikeRepository;
        private readonly IStorageService _storageService;
        private readonly IMapper _mapper;

        public GetAllProductQueryHandler(IProductRepository productRepository, IMapper mapper,
            IStorageService storageService, IProductLikeRepository productLikeRepository)
        {
            _productRepository = productRepository;
            _mapper = mapper;
            _storageService = storageService;
            _productLikeRepository = productLikeRepository;
        }

        public async Task<GetListResponse<GetAllProductQueryResponse>> Handle(GetAllProductQuery request,
            CancellationToken cancellationToken)
        {
            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                List<Product> products = await _productRepository.GetAllAsync(
                    include: p => p
                        .Include(p => p.Category)
                        .Include(p => p.Brand)
                        .Include(p => p.ProductLikes)
                        .Include(x => x.ProductFeatureValues).ThenInclude(x => x.FeatureValue)
                        .ThenInclude(x => x.Feature)
                        .Include(x => x.ProductImageFiles.Where(pif => pif.Showcase == true)),
                    cancellationToken: cancellationToken);

                GetListResponse<GetAllProductQueryResponse> response =
                    _mapper.Map<GetListResponse<GetAllProductQueryResponse>>(products);

                response.Items.SetImageUrls(_storageService);

                return response;
            }
            else
            {
                IPaginate<Product> products = await _productRepository.GetListAsync(
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    include: x => x.Include(x => x.Category)
                        .Include(x => x.Brand)
                        .Include(p => p.ProductLikes)
                        .Include(x => x.ProductFeatureValues).ThenInclude(x => x.FeatureValue)
                        .ThenInclude(x => x.Feature)
                        .Include(x => x.ProductImageFiles.Where(pif => pif.Showcase == true)),
                    cancellationToken: cancellationToken
                );
                GetListResponse<GetAllProductQueryResponse> response =
                    _mapper.Map<GetListResponse<GetAllProductQueryResponse>>(products);

                response.Items.SetImageUrls(_storageService);
                return response;
            }
        }

        
    }
}