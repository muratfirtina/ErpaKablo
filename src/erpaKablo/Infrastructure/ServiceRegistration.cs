using Application.Abstraction.Services;
using Application.Abstraction.Services.Configurations;
using Application.Services;
using Application.Storage;
using Application.Storage.Cloudinary;
using Application.Storage.Google;
using Application.Storage.Local;
using Application.Storage.Yandex;
using Application.Tokens;
using Infrastructure.BackgroundJobs;
using Infrastructure.Enums;
using Infrastructure.Logging.Enrichers;
using Infrastructure.Services;
using Infrastructure.Services.Cache;
using Infrastructure.Services.Configurations;
using Infrastructure.Services.Mail;
using Infrastructure.Services.Monitoring;
using Infrastructure.Services.Seo;
using Infrastructure.Services.Storage;
using Infrastructure.Services.Storage.Cloudinary;
using Infrastructure.Services.Storage.Google;
using Infrastructure.Services.Storage.Local;
using Infrastructure.Services.Storage.Yandex;
using Infrastructure.Services.Token;
using Infrastructure.Settings.Models.Newsletter;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Persistence.Services;
using Prometheus;
using Quartz;
using StackExchange.Redis;

namespace Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        services.Configure<StorageSettings>(configuration.GetSection("Storage"));
        
        // Storage providers
        services.AddScoped<ILocalStorage, LocalStorage>();
        services.AddScoped<ICloudinaryStorage, CloudinaryStorage>();
        services.AddScoped<IGoogleStorage, GoogleStorage>();
        //services.AddScoped<IYandexStorage, YandexStorage>();
        
        // Storage factory ve service
        services.AddScoped<IStorageProviderFactory, StorageProviderFactory>();
        services.AddScoped<IStorageService, StorageService>();
        
        // Diğer servisler
        services.AddScoped<IFileNameService, FileNameService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<ITokenHandler, TokenHandler>();
        services.AddScoped<ILogService, LogService>();
        services.AddScoped<ICompanyAssetService, CompanyAssetService>();
        
        services.Configure<NewsletterSettings>(configuration.GetSection("Newsletter"));
        services.AddScoped<INewsletterService, NewsletterService>();
        
        services.AddScoped<IImageSeoService, ImageSeoService>();
        services.AddScoped<ISitemapService, SitemapService>();
        // Quartz yapılandırması
        services.AddQuartz(q =>
        {
            var jobKey = new JobKey("MonthlyNewsletterJob");
            q.AddJob<MonthlyNewsletterJob>(opts => opts.WithIdentity(jobKey));
            
            var newsletterConfig = configuration.GetSection("Newsletter:SendTime")
                .Get<NewsletterSendTimeConfig>();
            
            if (newsletterConfig == null)
            {
                newsletterConfig = new NewsletterSendTimeConfig(); // Varsayılan değerler kullanılır
            }
            
            q.AddTrigger(opts => opts
                .ForJob(jobKey)
                .WithIdentity("MonthlyNewsletterJob-trigger")
                .WithCronSchedule($"0 {newsletterConfig.Minute} {newsletterConfig.Hour} {newsletterConfig.DayOfMonth} * ?"));
        });

        services.AddQuartzHostedService(q => q.WaitForJobsToComplete = true);
        
        return services;
    }

    
}