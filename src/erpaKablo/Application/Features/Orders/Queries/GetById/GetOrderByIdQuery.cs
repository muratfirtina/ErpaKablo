using Application.Extensions;
using Application.Features.Orders.Dtos;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Orders.Queries.GetById;

public class GetOrderByIdQuery : IRequest<GetOrderByIdQueryResponse>
{
    public string Id { get; set; }
    
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery,GetOrderByIdQueryResponse>
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
            var order = await _orderRepository.GetAsync(
                predicate: x => x.Id == request.Id,
                include: x => x
                    .Include(x => x.OrderItems).ThenInclude(x => x.Product).ThenInclude(x => x.ProductImageFiles)
                    .Include(x => x.OrderItems).ThenInclude(x => x.Product).ThenInclude(x => x.Brand)
                    .Include(x => x.OrderItems).ThenInclude(x => x.Product).ThenInclude(x => x.ProductFeatureValues).ThenInclude(x => x.FeatureValue).ThenInclude(x => x.Feature)
                    .Include(x => x.User) // Kullanıcı bilgilerini dahil ediyoruz
                    .Include(x => x.UserAddress), // Adres bilgilerini dahil ediyoruz
                cancellationToken: cancellationToken);

                
            if (order == null)
                throw new Exception($"Order not found with ID: {request.Id}");
            
            var response = _mapper.Map<GetOrderByIdQueryResponse>(order);
            
            response.OrderItems.SetImageUrls(_storageService);
            
            return response;
        }
    }
}