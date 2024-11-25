using Application.Abstraction.Services;
using Application.Abstraction.Services.HubServices;
using Domain.Enum;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using SignalR.Hubs;

namespace SignalR.HubService;

public class OrderHubService : IOrderHubService
{
    private readonly IHubContext<OrderHub> _hubContext; 
    private readonly ILogger<OrderHubService> _logger;

    public OrderHubService(IHubContext<OrderHub> hubContext, ILogger<OrderHubService> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task OrderCreatedMessageAsync(string orderId, string message)
    {
        try
        {
            var notification = new
            {
                Type = "OrderCreated",
                OrderId = orderId,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group("Admins")
                .SendAsync(ReceiveFunctionNames.ReceiveOrderCreated, message);

            _logger.LogInformation(
                "Order creation notification sent. OrderId: {OrderId}", orderId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error sending order creation notification. OrderId: {OrderId}", orderId);
            throw;
        }
    }

    public async Task OrderStausChangedMessageAsync(string orderId, string status, string message)
    {
        try
        {
            var notification = new
            {
                Type = "OrderStatusUpdate",
                OrderId = orderId,
                Status = status,
                Message = message,
                Timestamp = DateTime.UtcNow
            };

            await _hubContext.Clients.Group("Admins")
                .SendAsync(ReceiveFunctionNames.ReceiveOrderStatusUpdate, notification);

            _logger.LogInformation(
                "Order status update notification sent. OrderId: {OrderId}, Status: {Status}", 
                orderId, status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, 
                "Error sending order status update notification. OrderId: {OrderId}", orderId);
            throw;
        }
    }
}