using Application.Extensions;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries.GetById;

public class GetOrderByIdQuery : IRequest<GetOrderByIdQueryResponse>
{
    public string Id { get; set; }

    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, GetOrderByIdQueryResponse>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;

        public GetOrderByIdQueryHandler(IOrderRepository orderRepository, IMapper mapper, IStorageService storageService)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _storageService = storageService;
        }

        public async Task<GetOrderByIdQueryResponse> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            // Siparişi veritabanından getiriyoruz ve ilgili ilişkileri (OrderItems ve Product) yüklüyoruz
            var order = await _orderRepository.GetAsync(
                predicate: o => o.Id == request.Id,
                include: o => o
                    .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.ProductImageFiles)
                    .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.Brand)
                    .Include(o => o.OrderItems).ThenInclude(oi => oi.Product).ThenInclude(p => p.ProductFeatureValues).ThenInclude(pfv => pfv.FeatureValue).ThenInclude(fv => fv.Feature)
                    .Include(o => o.User) // Kullanıcı bilgilerini dahil ediyoruz
                    .Include(o => o.UserAddress)
                    .Include(o => o.PhoneNumber), // Adres bilgilerini dahil ediyoruz
                cancellationToken: cancellationToken
            );

            if (order == null)
                throw new Exception($"Sipariş bulunamadı: {request.Id}");

            // Order'dan GetOrderByIdQueryResponse DTO'suna dönüştürüyoruz
            var response = _mapper.Map<GetOrderByIdQueryResponse>(order);

            // Sipariş öğelerinin resim URL'lerini ayarlıyoruz (resim dosyalarını dinamik olarak alıyoruz)
            response.OrderItems.SetImageUrls(_storageService);

            return response;
        }
    }
}