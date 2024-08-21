using Application.Features.Brands.Dtos;
using Application.Features.Brands.Rules;
using Application.Repositories;
using Application.Storage;
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
        private readonly IStorageService _storageService;

        public GetListByDynamicBrandQueryHandler(IBrandRepository brandRepository, IMapper mapper, BrandBusinessRules brandBusinessRules, IStorageService storageService)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
            _brandBusinessRules = brandBusinessRules;
            _storageService = storageService;
        }

        public async Task<GetListResponse<GetListBrandByDynamicDto>> Handle(GetListBrandByDynamicQuery request, CancellationToken cancellationToken)
        {

            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                var allBrands = await _brandRepository.GetAllByDynamicAsync(
                    request.DynamicQuery,
                    include: x => x.Include(c => c.BrandImageFiles),
                    cancellationToken: cancellationToken);

                var brandsDtos = _mapper.Map<GetListResponse<GetListBrandByDynamicDto>>(allBrands);
                SetBrandImageUrls(brandsDtos.Items);
                return brandsDtos;
            }
            else
            {
                IPaginate<Brand> brands = await _brandRepository.GetListByDynamicAsync(
                    request.DynamicQuery,
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    include: x => x.Include(c => c.BrandImageFiles),
                    cancellationToken: cancellationToken);
                
                var brandsDtos = _mapper.Map<GetListResponse<GetListBrandByDynamicDto>>(brands);
                SetBrandImageUrls(brandsDtos.Items);
                return brandsDtos;

            }
        }
        
        private void SetBrandImageUrls(IEnumerable<GetListBrandByDynamicDto> brands)
        {
            var baseUrl = _storageService.GetStorageUrl();
            foreach (var brand in brands)
            {
                if (brand.BrandImage != null)
                {
                    brand.BrandImage.Url = $"{baseUrl}{brand.BrandImage.EntityType}/{brand.BrandImage.Path}/{brand.BrandImage.FileName}";
                }
                else
                {
                    brand.BrandImage = new BrandImageFileDto
                    {
                        EntityType = "brands",
                        Path = "",
                        FileName = "default-brand-image.png",
                        Url = $"{baseUrl}brands/default-brand-image.png"
                    };
                }
            }
        }
    }
}