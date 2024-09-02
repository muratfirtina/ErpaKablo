using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Paging;
using Domain;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Carousels.Queries.GetCarousel;

public class GetAllCarouselQuery : IRequest<GetListResponse<GetAllCarouselQueryResponse>>
{
    public PageRequest PageRequest { get; set; }

    public class GetCarouselQueryHandler : IRequestHandler<GetAllCarouselQuery, GetListResponse<GetAllCarouselQueryResponse>>
    {
        private readonly ICarouselRepository _carouselRepository;
        private readonly IStorageService _storageService;
        private readonly IMapper _mapper;

        public GetCarouselQueryHandler(ICarouselRepository carouselRepository, IStorageService storageService, IMapper mapper)
        {
            _carouselRepository = carouselRepository;
            _storageService = storageService;
            _mapper = mapper;
        }

        public async Task<GetListResponse<GetAllCarouselQueryResponse>> Handle(GetAllCarouselQuery request, CancellationToken cancellationToken)
        {
            if (request.PageRequest.PageIndex == -1 && request.PageRequest.PageSize == -1)
            {
                List<Carousel> carousels = await _carouselRepository.GetAllAsync(
                    include: x => x
                        .Include(x => x.CarouselImageFiles),
                    cancellationToken: cancellationToken);
                GetListResponse<GetAllCarouselQueryResponse> response = _mapper.Map<GetListResponse<GetAllCarouselQueryResponse>>(carousels);
                SetCarouselImageUrls(response.Items);
                return response;
            }
            else
            {
                IPaginate<Carousel> carousels = await _carouselRepository.GetListAsync(
                    index: request.PageRequest.PageIndex,
                    size: request.PageRequest.PageSize,
                    include: x => x
                        .Include(x => x.CarouselImageFiles),
                    cancellationToken: cancellationToken
                );
                GetListResponse<GetAllCarouselQueryResponse> response = _mapper.Map<GetListResponse<GetAllCarouselQueryResponse>>(carousels);
                SetCarouselImageUrls(response.Items);
                return response;
            }
        }

        private void SetCarouselImageUrls(IEnumerable<GetAllCarouselQueryResponse> carousels)
        {
            var baseUrl = _storageService.GetStorageUrl();
            foreach (var carousel in carousels)
            {
                if (carousel.CarouselImageFiles != null)
                {
                    foreach (var carouselImageFile in carousel.CarouselImageFiles)
                    {
                        carouselImageFile.Url = $"{baseUrl}{carouselImageFile.EntityType}/{carouselImageFile.Path}/{carouselImageFile.FileName}";
                    }
                }
            }
        }
    }
}
