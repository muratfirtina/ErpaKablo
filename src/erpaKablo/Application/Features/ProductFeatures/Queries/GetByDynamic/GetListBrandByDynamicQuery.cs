using Application.Features.ProductFeatures.Rules;
using Application.Repositories;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.ProductFeatures.Queries.GetByDynamic;

public class GetListProductFeatureByDynamicQuery : IRequest<GetListResponse<GetListProductFeatureByDynamicDto>>
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery DynamicQuery { get; set; }
    
    public class GetListByDynamicProductFeatureQueryHandler : IRequestHandler<GetListProductFeatureByDynamicQuery, GetListResponse<GetListProductFeatureByDynamicDto>>
    {
        private readonly IProductFeatureRepository _productFeatureRepository;
        private readonly IMapper _mapper;
        private readonly ProductFeatureBusinessRules _productFeatureBusinessRules;

        public GetListByDynamicProductFeatureQueryHandler(IProductFeatureRepository productFeatureRepository, IMapper mapper, ProductFeatureBusinessRules productFeatureBusinessRules)
        {
            _productFeatureRepository = productFeatureRepository;
            _mapper = mapper;
            _productFeatureBusinessRules = productFeatureBusinessRules;
        }

        public async Task<GetListResponse<GetListProductFeatureByDynamicDto>> Handle(GetListProductFeatureByDynamicQuery request, CancellationToken cancellationToken)
        {
            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                var allProductFeatures = await _productFeatureRepository.GetAllByDynamicAsync(
                    request.DynamicQuery,
                    cancellationToken: cancellationToken);

                var productFeaturesDtos = _mapper.Map<GetListResponse<GetListProductFeatureByDynamicDto>>(allProductFeatures);
                return productFeaturesDtos;
            }
            else
            {
                IPaginate<ProductFeature> productFeatures = await _productFeatureRepository.GetListByDynamicAsync(
                    request.DynamicQuery,
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    cancellationToken: cancellationToken);
                
                var productFeaturesDtos = _mapper.Map<GetListResponse<GetListProductFeatureByDynamicDto>>(productFeatures);
                return productFeaturesDtos;

            }
        }
    }
}