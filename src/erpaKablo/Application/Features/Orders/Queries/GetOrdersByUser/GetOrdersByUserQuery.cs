using Application.Extensions;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.Application.Requests;
using Core.Application.Responses;
using Core.Persistence.Dynamic;
using Core.Persistence.Paging;
using Domain;
using Domain.Enum;
using Domain.Identity;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries.GetOrdersByUser;
public class GetOrdersByUserQuery : IRequest<GetListResponse<GetOrdersByUserQueryResponse>>
{
    public PageRequest PageRequest { get; set; }
    public string? SearchTerm { get; set; }
    public string? DateRange { get; set; }
    public OrderStatus OrderStatus { get; set; }
    
    public class GetOrdersByUserQueryHandler : IRequestHandler<GetOrdersByUserQuery, GetListResponse<GetOrdersByUserQueryResponse>>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMapper _mapper;
        private readonly IStorageService _storageService;

        public GetOrdersByUserQueryHandler(IOrderRepository orderRepository, IMapper mapper, IStorageService storageService)
        {
            _orderRepository = orderRepository;
            _mapper = mapper;
            _storageService = storageService;
            
        }

        public async Task<GetListResponse<GetOrdersByUserQueryResponse>> Handle(GetOrdersByUserQuery request, CancellationToken cancellationToken)
        {
            // Kullanıcının siparişlerini alıyoruz
            IPaginate<Order> orders = await _orderRepository.GetOrdersByUserAsync(request.PageRequest, request.OrderStatus, request.DateRange, request.SearchTerm);

            // IPaginate<Order>'ı GetListResponse<GetOrdersByUserQueryResponse>'a mapliyoruz
            GetListResponse<GetOrdersByUserQueryResponse> response = _mapper.Map<GetListResponse<GetOrdersByUserQueryResponse>>(orders);
            
            // Sipariş öğelerinin resim URL'lerini ayarlıyoruz (resim dosyalarını dinamik olarak alıyoruz)
            response.Items.Where(x => x.OrderItems != null).ToList().ForEach(x => x.OrderItems.SetImageUrls(_storageService));
            
            
            return response; // Sonuçları döndürüyoruz
        }
    }
}