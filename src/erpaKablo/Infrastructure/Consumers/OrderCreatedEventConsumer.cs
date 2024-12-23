using Application.Abstraction.Services;
using Application.Abstraction.Services.HubServices;
using Application.Events.OrderEvetns;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Consumers;

public class OrderCreatedEventConsumer : IConsumer<OrderCreatedEvent>
{
    private readonly IMailService _mailService;
    private readonly IOrderHubService _orderHubService;
    private readonly ILogger<OrderCreatedEventConsumer> _logger;

    public OrderCreatedEventConsumer(
        IMailService mailService,
        IOrderHubService orderHubService,
        ILogger<OrderCreatedEventConsumer> logger)
    {
        _mailService = mailService;
        _orderHubService = orderHubService;
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<OrderCreatedEvent> context)
    {
        var order = context.Message;
        _logger.LogInformation($"Order created event received: {order.OrderId}");

        try
        {
            // Mail gönderimi
            await _mailService.SendCreatedOrderEmailAsync(
                order.Email,
                order.OrderCode,
                order.Description,
                order.UserAddress,
                order.OrderDate,
                order.UserName,
                order.OrderItems,
                order.TotalPrice
            );

            // SignalR bildirimi
            await _orderHubService.OrderCreatedMessageAsync(order.OrderId, "Sipariş oluşturuldu.");

            _logger.LogInformation($"Order {order.OrderId} processing completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error processing order {order.OrderId}");
            throw; // MassTransit retry mekanizmasını tetikler
        }
    }
}