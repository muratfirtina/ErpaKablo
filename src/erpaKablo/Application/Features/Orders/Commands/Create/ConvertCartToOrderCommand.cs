using Application.Abstraction.Services;
using Application.Abstraction.Services.HubServices;
using Application.Events.OrderEvetns;
using Application.Extensions;
using Application.Features.Carts.Dtos;
using Application.Features.Orders.Dtos;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using MassTransit;
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
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly IMailService _mailService;
        private readonly IOrderHubService _orderHubService;

        public ConvertCartToOrderCommandHandler(
            IOrderRepository orderRepository,
            IPublishEndpoint publishEndpoint,
            IMailService mailService,
            IOrderHubService orderHubService)
        {
            _orderRepository = orderRepository;
            _publishEndpoint = publishEndpoint;
            _mailService = mailService;
            _orderHubService = orderHubService;
        }

        public async Task<ConvertCartToOrderCommandResponse> Handle(ConvertCartToOrderCommand request, CancellationToken cancellationToken)
        {
            (bool succeeded, OrderDto? orderDto) = await _orderRepository.ConvertCartToOrderAsync(
                request.AddressId,
                request.PhoneNumberId,
                request.Description
            );

            if (!succeeded || orderDto == null)
            {
                throw new Exception("Sepet siparişe dönüştürülemedi.");
            }

            // OrderCreated eventini yayınla
            await _publishEndpoint.Publish(new OrderCreatedEvent
            {
                OrderId = orderDto.OrderId,
                OrderCode = orderDto.OrderCode,
                Email = orderDto.Email,
                UserName = orderDto.UserName,
                OrderDate = orderDto.OrderDate,
                OrderItems = orderDto.OrderItems,
                UserAddress = orderDto.UserAddress,
                TotalPrice = orderDto.TotalPrice,
                Description = orderDto.Description
            }, cancellationToken);

            // SignalR bildirimi
            await _orderHubService.OrderCreatedMessageAsync(orderDto.OrderId, "Sipariş oluşturuldu.");

            return new ConvertCartToOrderCommandResponse
            {
                OrderId = orderDto.OrderId
            };
        }
    }
}
