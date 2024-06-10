using Application.Features.Brands.Rules;
using Application.Repositories;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Brands.Queries.GetByDynamic;

public class GetListBrandByDynamicQuery : IRequest<GetListResponse<GetListBrandByDynamicDto>>
{
    public PageRequest PageRequest { get; set; }
    public DynamicQuery DynamicQuery { get; set; }
    
    public class GetListByDynamicBrandQueryHandler : IRequestHandler<GetListBrandByDynamicQuery, GetListResponse<GetListBrandByDynamicDto>>
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IMapper _mapper;
        private readonly BrandBusinessRules _brandBusinessRules;

        public GetListByDynamicBrandQueryHandler(IBrandRepository brandRepository, IMapper mapper, BrandBusinessRules brandBusinessRules)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
            _brandBusinessRules = brandBusinessRules;
        }

        public async Task<GetListResponse<GetListBrandByDynamicDto>> Handle(GetListBrandByDynamicQuery request, CancellationToken cancellationToken)
        {

            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                var allBrands = await _brandRepository.GetAllByDynamicAsync(
                    request.DynamicQuery,
                    cancellationToken: cancellationToken);

                var brandsDtos = _mapper.Map<GetListResponse<GetListBrandByDynamicDto>>(allBrands);
                return brandsDtos;
            }
            else
            {
                IPaginate<Brand> brands = await _brandRepository.GetListByDynamicAsync(
                    request.DynamicQuery,
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    cancellationToken: cancellationToken);
                
                var brandsDtos = _mapper.Map<GetListResponse<GetListBrandByDynamicDto>>(brands);
                return brandsDtos;

            }
        }
    }
}