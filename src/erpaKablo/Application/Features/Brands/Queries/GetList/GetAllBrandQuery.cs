using Application.Features.Brands.Dtos;
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

namespace Application.Features.Brands.Queries.GetList;

public class GetAllBrandQuery : IRequest<GetListResponse<GetAllBrandQueryResponse>>
{
    public PageRequest PageRequest { get; set; }
    
    public class GetAllBrandQueryHandler : IRequestHandler<GetAllBrandQuery, GetListResponse<GetAllBrandQueryResponse>>
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;

        public GetAllBrandQueryHandler(IBrandRepository brandRepository, IMapper mapper, IStorageService storageService)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<GetListResponse<GetAllBrandQueryResponse>> Handle(GetAllBrandQuery request, CancellationToken cancellationToken)
        {
            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                List<Brand> brands = await _brandRepository.GetAllAsync(
                    include: x => x.Include(x => x.BrandImageFiles),
                    cancellationToken: cancellationToken);
                GetListResponse<GetAllBrandQueryResponse> response = _mapper.Map<GetListResponse<GetAllBrandQueryResponse>>(brands);
                SetBrandImageUrls(response.Items);
                return response;
            }
            else
            {
                IPaginate<Brand> brands = await _brandRepository.GetListAsync(
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    include: x => x.Include(x => x.BrandImageFiles),
                    cancellationToken: cancellationToken
                );
                GetListResponse<GetAllBrandQueryResponse> response = _mapper.Map<GetListResponse<GetAllBrandQueryResponse>>(brands);
                SetBrandImageUrls(response.Items);
                return response;
            }
        }

        private void SetBrandImageUrls(IEnumerable<GetAllBrandQueryResponse> brands)
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
