using Application.Extensions;
using Application.Features.Brands.Dtos;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.Application.Responses;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Brands.Queries.GetBrandsByIds;

public class GetBrandsByIdsQuery : IRequest<GetListResponse<GetBrandsByIdsQueryResponse>>
{
    public List<string> Ids { get; set; }
    
    public class GetBrandsByIdsQueryHandler : IRequestHandler<GetBrandsByIdsQuery, GetListResponse<GetBrandsByIdsQueryResponse>>
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;

        public GetBrandsByIdsQueryHandler(IBrandRepository brandRepository, IMapper mapper, IStorageService storageService)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<GetListResponse<GetBrandsByIdsQueryResponse>> Handle(GetBrandsByIdsQuery request, CancellationToken cancellationToken)
        {
            List<Brand> brands = await _brandRepository.GetAllAsync(
                index:-1,
                size:-1,
                predicate: x => request.Ids.Contains(x.Id),
                include: c => c
                    .Include(c => c.BrandImageFiles)
                    .Include(fv => fv.Products),
                cancellationToken: cancellationToken
            );

            GetListResponse<GetBrandsByIdsQueryResponse> response = _mapper.Map<GetListResponse<GetBrandsByIdsQueryResponse>>(brands);
            response.Items.SetImageUrls(_storageService);
            return response;
        }
        
    }
}