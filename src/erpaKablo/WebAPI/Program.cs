using System.Text;
using Application;
using Application.Services;
using HealthChecks.UI.Client;
using Infrastructure;
using Infrastructure.Configuration;
using Infrastructure.Services.Security;
using Infrastructure.Services.Seo;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Context;
using Persistence.DbConfiguration;
using Persistence.Services;
using Prometheus;
using Serilog;
using SignalR;
using SignalR.Hubs;

var builder = WebApplication.CreateBuilder(args);

try
{
    // 1. Temel Yapılandırmalar
    builder.AddKeyVaultConfiguration();
    builder.AddLoggingConfiguration();

    // 2. Servis Katmanları
    ConfigureServiceLayers(builder);

    // 3. Middleware ve Güvenlik
    ConfigureSecurityAndAuth(builder);
    
    // 4. Diğer Servisler
    ConfigureAdditionalServices(builder);

    var app = builder.Build();
    
    var initService = app.Services.GetRequiredService<IKeyVaultInitializationService>();
    await initService.InitializeAsync();
    await ConfigureApplication(app);
    
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}

// Service Layer Configuration Methods
void ConfigureServiceLayers(WebApplicationBuilder builder)
{
    // Core Services
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();
    builder.Services.AddHttpClient();
    builder.Services.AddHttpContextAccessor();

    // Infrastructure Layer
    builder.Services.AddInfrastructureServices(builder.Configuration);

    // Security Layer
    builder.Services.AddSecurityServices(builder.Configuration);

    // Application Layer
    builder.Services.AddApplicationServices();

    // Persistence Layer
    var loggerFactory = builder.Services.BuildServiceProvider()
        .GetRequiredService<ILoggerFactory>();
    var logger = loggerFactory.CreateLogger("DatabaseConfiguration");
    
    builder.Services.AddPersistenceServices(
        DatabaseConfiguration.GetConnectionString(builder.Configuration, logger),
        builder.Environment.IsDevelopment());

    // SignalR Services
    builder.Services.AddSignalRServices();
}

// Security and Authentication Configuration
void ConfigureSecurityAndAuth(WebApplicationBuilder builder)
{
    // CORS Configuration
    builder.Services.AddCors(options =>
    {
        options.AddDefaultPolicy(policy =>
        {
            policy.WithOrigins(builder.Configuration.GetSection("WebAPIConfiguration:AllowedOrigins").Get<string[]>())
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders(
                    "Content-Security-Policy",
                    "X-Content-Type-Options",
                    "X-Frame-Options",
                    "X-XSS-Protection",
                    //"Strict-Transport-Security",
                    "Referrer-Policy",
                    "Permissions-Policy"
                )
                .AllowCredentials();
        });
    });

    // Message Broker Configuration
    builder.Services.AddMessageBrokerServices(builder.Configuration);

    // Authentication Configuration
    builder.Services.AddAuthenticationServices(builder.Configuration);
}

// Additional Services Configuration
void ConfigureAdditionalServices(WebApplicationBuilder builder)
{
    // File Provider Configuration
    var provider = new FileExtensionContentTypeProvider();
    provider.Mappings[".avif"] = "image/avif";

    // Prometheus Metrics
    builder.Services.AddMetricServer(options => { options.Port = 9100; });
}

// Application Configuration
async Task ConfigureApplication(WebApplication app)
{
    // Environment specific configuration
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    else
    {
        app.UseHsts();
    }

    // Security Middleware
    app.UseSecurityMiddleware(app.Configuration);

    // Basic Middleware
    app.UseMetricServer();
    app.UseCors();
    app.UseHttpsRedirection();
    app.UseStaticFiles(new StaticFileOptions
    {
        ContentTypeProvider = new FileExtensionContentTypeProvider
        {
            Mappings = { [".avif"] = "image/avif" }
        }
    });
    
    app.UseMiddleware<ImageOptimizationMiddleware>();

    // Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Request Logging
    app.UseCustomRequestLogging();

    // API Routes
    app.MapControllers();
    app.MapHubs();

    // Health Checks
    ConfigureHealthChecks(app);

    // Database Migration
    await using (var scope = app.Services.CreateAsyncScope())
    {
        try
        {
            var context = scope.ServiceProvider.GetRequiredService<ErpaKabloDbContext>();
            await context.Database.MigrateAsync();
            await RoleAndUserSeeder.SeedAsync(scope.ServiceProvider);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "An error occurred during database initialization");
            throw;
        }
    }
}

// Health Checks Configuration
void ConfigureHealthChecks(WebApplication app)
{
    app.MapHealthChecks("/health/ready", new HealthCheckOptions
    {
        Predicate = check => check.Tags.Contains("ready"),
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });

    app.MapHealthChecks("/health/live", new HealthCheckOptions
    {
        Predicate = _ => false,
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
}