using Application.Abstraction.Services;
using Application.Abstraction.Services.HubServices;
using Application.Extensions;
using Application.Features.Carts.Dtos;
using Application.Features.Orders.Dtos;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using MediatR;

namespace Application.Features.Orders.Commands.Create;

public class ConvertCartToOrderCommand : IRequest<ConvertCartToOrderCommandResponse>
{
    public string? AddressId { get; set; }
    public string? PhoneNumberId { get; set; }
    public string? Description { get; set; }

    public class ConvertCartToOrderCommandHandler : IRequestHandler<ConvertCartToOrderCommand, ConvertCartToOrderCommandResponse>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IMailService _mailService;
        private readonly IMapper _mapper;
        private readonly IOrderHubService _orderHubService;
        private readonly IStorageService _storageService;

        public ConvertCartToOrderCommandHandler(IMapper mapper, IOrderRepository orderRepository, IMailService mailService, IOrderHubService orderHubService, IStorageService storageService)
        {
            _mapper = mapper;
            _orderRepository = orderRepository;
            _mailService = mailService;
            _orderHubService = orderHubService;
            _storageService = storageService;
        }

        public async Task<ConvertCartToOrderCommandResponse> Handle(ConvertCartToOrderCommand request, CancellationToken cancellationToken)
        {
            (bool succeeded, OrderDto? orderDto, List<CartItemDto>? newCartItems) = await _orderRepository.ConvertCartToOrderAsync(
                request.AddressId,
                request.PhoneNumberId,
                request.Description
            );

            if (!succeeded || orderDto == null)
            {
                throw new Exception("Sepet siparişe dönüştürülemedi.");
            }
    
            await _orderHubService.OrderCreatedMessageAsync(orderDto.OrderId, "Sipariş oluşturuldu.");

            try
            {
        
                await _mailService.SendCompletedOrderEmailAsync(
                    to: orderDto.Email,
                    orderCode: orderDto.OrderCode,
                    orderDescription: orderDto.Description,
                    orderAddress: orderDto.UserAddress,
                    orderCreatedDate: orderDto.OrderDate,
                    userName: orderDto.UserName,
                    orderCartItems: orderDto.OrderItems,
                    orderTotalPrice: orderDto.TotalPrice
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to send order confirmation email: {ex.Message}");
            }

            return new ConvertCartToOrderCommandResponse
            {
                OrderId = orderDto.OrderId
            };
        }
    }
}
