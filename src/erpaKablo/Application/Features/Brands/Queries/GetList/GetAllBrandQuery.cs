using Application.Repositories;
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

        public GetAllBrandQueryHandler(IBrandRepository brandRepository, IMapper mapper)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetAllBrandQueryResponse>> Handle(GetAllBrandQuery request, CancellationToken cancellationToken)
        {
            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                List<Brand> brands = await _brandRepository.GetAllAsync(
                    cancellationToken: cancellationToken);
                GetListResponse<GetAllBrandQueryResponse> response = _mapper.Map<GetListResponse<GetAllBrandQueryResponse>>(brands);
                return response;
            }
            else
            {
                IPaginate<Brand> brands = await _brandRepository.GetListAsync(
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    cancellationToken: cancellationToken
                );
                GetListResponse<GetAllBrandQueryResponse> response = _mapper.Map<GetListResponse<GetAllBrandQueryResponse>>(brands);
                return response;
            }
        }
    }
}