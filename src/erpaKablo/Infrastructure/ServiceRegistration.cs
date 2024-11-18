using Application.Abstraction.Services;
using Application.Abstraction.Services.Configurations;
using Application.Services;
using Application.Storage;
using Application.Storage.Cloudinary;
using Application.Storage.Google;
using Application.Storage.Local;
using Application.Tokens;
using Infrastructure.Enums;
using Infrastructure.Logging.Enrichers;
using Infrastructure.Services;
using Infrastructure.Services.Cache;
using Infrastructure.Services.Configurations;
using Infrastructure.Services.Mail;
using Infrastructure.Services.Monitoring;
using Infrastructure.Services.Storage;
using Infrastructure.Services.Storage.Cloudinary;
using Infrastructure.Services.Storage.Google;
using Infrastructure.Services.Storage.Local;
using Infrastructure.Services.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prometheus;
using StackExchange.Redis;

namespace Infrastructure;

public static class InfrastructureServiceRegistration
{
    
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<StorageSettings>(configuration.GetSection("Storage"));
        
        services.AddScoped<ILocalStorage, LocalStorage>();
        services.AddScoped<ICloudinaryStorage, CloudinaryStorage>();
        services.AddScoped<IGoogleStorage, GoogleStorage>();
        services.AddScoped<IStorageService, StorageService>();
        services.AddScoped<IFileNameService, FileNameService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<ITokenHandler, TokenHandler>();
        services.AddScoped<ILogService, LogService>();
        
        
        return services;
    }
    
    
    public static void AddStorage<T>(this IServiceCollection serviceCollection) where T : class,IBlobService
    {
        serviceCollection.AddScoped<IBlobService, T>();
        serviceCollection.AddScoped<IStorageService,StorageService>();
    }
    public static void AddStorage(this IServiceCollection serviceCollection, StorageType storageType)
    {
        switch (storageType)
        {
            case StorageType.Local:
                serviceCollection.AddScoped<IBlobService, LocalStorage>();
                break;
            
            case StorageType.Cloudinary:
                serviceCollection.AddScoped<IBlobService, CloudinaryStorage>();
                break;

            case StorageType.Google:
                serviceCollection.AddScoped<IBlobService, GoogleStorage>();
                break;
            default:
                serviceCollection.AddScoped<IBlobService, LocalStorage>();
                break;
        }
    }
}