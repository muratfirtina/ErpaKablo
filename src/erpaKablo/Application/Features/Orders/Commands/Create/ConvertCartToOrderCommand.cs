using System.Text.Json;
using Application.Abstraction.Services;
using Application.Abstraction.Services.HubServices;
using Application.Events.OrderEvetns;
using Application.Extensions;
using Application.Features.Carts.Dtos;
using Application.Features.Orders.Dtos;
using Application.Repositories;
using Application.Storage;
using AutoMapper;
using Core.Application.Pipelines.Caching;
using Core.Application.Pipelines.Transaction;
using Domain;
using MassTransit;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Application.Features.Orders.Commands.Create;

public class ConvertCartToOrderCommand : IRequest<ConvertCartToOrderCommandResponse>,ITransactionalRequest,ICacheRemoverRequest
{
    public string? AddressId { get; set; }
    public string? PhoneNumberId { get; set; }
    public string? Description { get; set; }
    
    public string CacheKey => "";
    public bool BypassCache => false;
    public string? CacheGroupKey => "Orders";

    public class ConvertCartToOrderCommandHandler : IRequestHandler<ConvertCartToOrderCommand, ConvertCartToOrderCommandResponse>
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IOutboxRepository _outboxRepository;
        private readonly ILogger<ConvertCartToOrderCommandHandler> _logger;

        public ConvertCartToOrderCommandHandler(IOrderRepository orderRepository, IOutboxRepository outboxRepository, ILogger<ConvertCartToOrderCommandHandler> logger)
        {
            _orderRepository = orderRepository;
            _outboxRepository = outboxRepository;
            _logger = logger;
        }

        public async Task<ConvertCartToOrderCommandResponse> Handle(ConvertCartToOrderCommand request, CancellationToken cancellationToken)
        {
            // 1. Sipariş oluşturma (Transaction içinde)
            (bool succeeded, OrderDto? orderDto) = await _orderRepository.ConvertCartToOrderAsync(
                request.AddressId,
                request.PhoneNumberId,
                request.Description
            );

            if (!succeeded || orderDto == null)
            {
                throw new Exception("Sepet siparişe dönüştürülemedi.");
            }

            // 2. Event'i Outbox'a kaydet (Transaction içinde)
            var outboxMessage = new OutboxMessage(
                nameof(OrderCreatedEvent),
                JsonSerializer.Serialize(new OrderCreatedEvent
                {
                    OrderId = orderDto.OrderId,
                    OrderCode = orderDto.OrderCode,
                    OrderDate = orderDto.OrderDate,
                    Description = request.Description,
                    UserAddress = orderDto.UserAddress,
                    UserName = orderDto.UserName,
                    OrderItems = orderDto.OrderItems,
                    TotalPrice = orderDto.TotalPrice,
                    Email = orderDto.Email
                })
            );

            await _outboxRepository.AddAsync(outboxMessage);

            return new ConvertCartToOrderCommandResponse
            {
                OrderId = orderDto.OrderId
            };
        }
    }
    
}
