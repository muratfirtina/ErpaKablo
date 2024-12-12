using Application.Abstraction.Services;
using Application.Abstraction.Services.HubServices;
using Microsoft.Extensions.DependencyInjection;
using SignalR.HubService;

namespace SignalR;

public static class ServiceRegistration
{
    public static void AddSignalRServices(this IServiceCollection services)
    {
        services.AddTransient<IOrderHubService, OrderHubService>();
        services.AddSignalR(options =>
        {
            options.EnableDetailedErrors = true;
        });
        
    }
}