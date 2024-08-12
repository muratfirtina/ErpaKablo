using Application.Abstraction.Services;
using Application.Abstraction.Services.Configurations;
using Application.Services;
using Application.Storage;
using Application.Storage.Cloudinary;
using Application.Storage.Google;
using Application.Storage.Local;
using Application.Tokens;
using Infrastructure.Enums;
using Infrastructure.Services;
using Infrastructure.Services.Configurations;
using Infrastructure.Services.Storage;
using Infrastructure.Services.Storage.Cloudinary;
using Infrastructure.Services.Storage.Google;
using Infrastructure.Services.Storage.Local;
using Infrastructure.Services.Token;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

public static class InfrastructureServiceRegistration
{
    
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ILocalStorage, LocalStorage>();
        services.AddScoped<ICloudinaryStorage, CloudinaryStorage>();
        services.AddScoped<IGoogleStorage, GoogleStorage>();
        services.AddScoped<IStorageService, StorageService>();
        services.AddScoped<IFileNameService, FileNameService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<ITokenHandler, TokenHandler>();
        services.AddScoped<IMailService, MailService>();
        
        services.Configure<StorageSettings>(configuration.GetSection("StorageUrls"));
        
        
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