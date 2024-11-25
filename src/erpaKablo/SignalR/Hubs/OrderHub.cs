using Application.Abstraction.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;

namespace SignalR.Hubs;

public class OrderHub : Hub
{
    private readonly ILogger<OrderHub> _logger;
    private readonly IUserService _userService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public OrderHub(
        ILogger<OrderHub> logger,
        IUserService userService,
        IHttpContextAccessor httpContextAccessor)
    {
        _logger = logger;
        _userService = userService; 
        _httpContextAccessor = httpContextAccessor;
    }

    public override async Task OnConnectedAsync()
    {
        try
        {
            var username = _httpContextAccessor.HttpContext?.User?.Identity?.Name;
            if (!string.IsNullOrEmpty(username))
            {
                bool isAdmin = await _userService.IsAdminAsync();
                if (isAdmin)
                {
                    await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
                    _logger.LogInformation($"User {username} added to Admins group");
                    await base.OnConnectedAsync();
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnConnectedAsync");
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        try
        {
            var user = Context.User;
            if (user?.Identity?.IsAuthenticated == true)
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
                _logger.LogInformation($"User {user.Identity.Name} removed from Admins group");
            }
            await base.OnDisconnectedAsync(exception);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in OnDisconnectedAsync");
            throw;
        }
    }

    public async Task JoinAdminGroup()
    {
        var user = Context.User;
        if (user?.Identity?.IsAuthenticated != true)
        {
            throw new HubException("Unauthorized");
        }

        // Veritabanından admin kontrolü
        bool isAdmin = await _userService.IsAdminAsync();
        if (!isAdmin)
        {
            throw new HubException("User is not an admin");
        }

        await Groups.AddToGroupAsync(Context.ConnectionId, "Admins");
        _logger.LogInformation($"User {user.Identity.Name} manually joined Admins group");
    }

    public async Task LeaveAdminGroup()
    {
        var user = Context.User;
        if (user?.Identity?.IsAuthenticated == true)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Admins");
            _logger.LogInformation($"User {user.Identity.Name} left Admins group");
        }
    }
}