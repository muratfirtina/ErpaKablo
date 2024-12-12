using Application.Extensions.ImageFileExtensions;
using Application.Features.Brands.Dtos;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.Application.Responses;
using MediatR;

namespace Application.Features.Brands.Queries.Search;

public class SearchBrandQuery : IRequest<GetListResponse<BrandDto>>
{
    public string SearchTerm { get; set; }

    public class SearchBrandQueryHandler : IRequestHandler<SearchBrandQuery, GetListResponse<BrandDto>>
    {
        private readonly IBrandRepository _brandRepository;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;

        public SearchBrandQueryHandler(
            IBrandRepository brandRepository, 
            IMapper mapper,
            IStorageService storageService)
        {
            _brandRepository = brandRepository;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<GetListResponse<BrandDto>> Handle(SearchBrandQuery request, CancellationToken cancellationToken)
        {
            var brands = await _brandRepository.SearchByNameAsync(request.SearchTerm);
            var response = _mapper.Map<GetListResponse<BrandDto>>(brands);

            // Her marka için görsel dönüşümü yap
            foreach (var brandDto in response.Items)
            {
                var brand = brands.Items.FirstOrDefault(b => b.Id == brandDto.Id);
                if (brand?.BrandImageFiles != null)
                {
                    var brandImage = brand.BrandImageFiles.FirstOrDefault();
                    if (brandImage != null)
                    {
                        brandDto.BrandImage = brandImage.ToDto(_storageService);
                    }
                }
            }

            return response;
        }
    }
}