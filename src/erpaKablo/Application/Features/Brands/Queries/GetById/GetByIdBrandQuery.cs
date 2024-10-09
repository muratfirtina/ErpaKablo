using Application.Extensions;
using Application.Features.Brands.Dtos;
using Application.Features.Brands.Queries.GetById;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.CrossCuttingConcerns.Exceptions;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

public class GetByIdBrandQuery : IRequest<GetByIdBrandResponse>
{
    public string Id { get; set; }
    
    public class GetByIdBrandQueryHandler : IRequestHandler<GetByIdBrandQuery, GetByIdBrandResponse>
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;

        public GetByIdBrandQueryHandler(IBrandRepository brandRepository, IMapper mapper, IStorageService storageService)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<GetByIdBrandResponse> Handle(GetByIdBrandQuery request, CancellationToken cancellationToken)
        {
            Brand? brand = await _brandRepository.GetAsync(
                predicate: p => p.Id == request.Id,
                include: x => x.Include(x => x.BrandImageFiles),
                cancellationToken: cancellationToken);

            if (brand == null)
            {
                throw new BusinessException("Brand not found");
            }

            GetByIdBrandResponse response = _mapper.Map<GetByIdBrandResponse>(brand);
            response.SetImageUrl(_storageService);

            return response;
        }
    }
}