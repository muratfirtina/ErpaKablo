using System.Security.Claims;
using System.Text;
using Application;
using Application.Extensions;
using Azure.Identity;
using HealthChecks.UI.Client;
using Infrastructure;
using Infrastructure.Consumers;
using Infrastructure.Middleware.DDosProtection;
using Infrastructure.Middleware.Monitoring;
using Infrastructure.Middleware.RateLimiting;
using Infrastructure.Middleware.Security;
using Infrastructure.Services.Monitoring;
using Infrastructure.Services.Security;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.IdentityModel.Tokens;
using Persistence;
using Persistence.Services;
using Prometheus;
using Serilog;
using Serilog.Formatting.Json;
using SignalR;

var builder = WebApplication.CreateBuilder(args);

// Environment-based Configuration
if (builder.Environment.IsProduction())
{
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{builder.Configuration["AzureKeyVault:VaultName"]}.vault.azure.net/"),
        new ClientSecretCredential(
            builder.Configuration["AzureKeyVault:TenantId"],
            builder.Configuration["AzureKeyVault:ClientId"],
            builder.Configuration["AzureKeyVault:ClientSecret"]));
}

var provider = new FileExtensionContentTypeProvider();
provider.Mappings[".avif"] = "image/avif";

// Serilog Configuration
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .Enrich.WithMachineName()
    .Enrich.WithEnvironmentName()
    .WriteTo.Console(new JsonFormatter())
    .WriteTo.Seq(
        serverUrl: builder.Configuration["Serilog:WriteTo:1:Args:serverUrl"] ?? "http://localhost:5341",
        restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information
    )
    .CreateLogger();

builder.Host.UseSerilog();

// Core Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();
builder.Services.AddHttpContextAccessor();
builder.Services.AddMemoryCache();

// CORS Configuration
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("WebAPIConfiguration:AllowedOrigins").Get<string[]>())
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});

// Infrastructure Layer
builder.Services.AddInfrastructureServices(builder.Configuration);

// Security Layer
builder.Services.AddSecurityServices(builder.Configuration);

// Application Layer
builder.Services.AddApplicationServices();

// Custom Behaviors
builder.Services.AddCustomBehaviors();

// Persistence Layer
builder.Services.AddPersistenceServices();

// SignalR Services
builder.Services.AddSignalRServices();

// Prometheus Metrics
builder.Services.AddMetricServer(options => { options.Port = 9100; });

builder.Services.AddMassTransit(x =>
{
    // Consumer'ları kaydet
    x.AddConsumer<OrderCreatedEventConsumer>();
    x.AddConsumer<CartUpdatedEventConsumer>();
    x.AddConsumer<StockUpdatedEventConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        // RabbitMQ bağlantı ayarları
        cfg.Host(builder.Configuration["RabbitMQ:Host"], "/", h =>
        {
            h.Username(builder.Configuration["RabbitMQ:Username"]);
            h.Password(builder.Configuration["RabbitMQ:Password"]);
        });

        // OrderCreated queue yapılandırması
        cfg.ReceiveEndpoint(builder.Configuration["RabbitMQ:Queues:OrderCreated"], e =>
        {
            // Consumer'ı yapılandır
            e.ConfigureConsumer<OrderCreatedEventConsumer>(context);

            // Retry policy
            e.UseMessageRetry(r =>
            {
                r.Interval(
                    int.Parse(builder.Configuration["RabbitMQ:RetryCount"]), 
                    TimeSpan.FromSeconds(double.Parse(builder.Configuration["RabbitMQ:RetryInterval"]))
                );
            });

            // Prefetch count
            e.PrefetchCount = int.Parse(builder.Configuration["RabbitMQ:PrefetchCount"]);
        });

        // CartUpdated queue yapılandırması
        cfg.ReceiveEndpoint(builder.Configuration["RabbitMQ:Queues:CartUpdated"], e =>
        {
            e.ConfigureConsumer<CartUpdatedEventConsumer>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
        });

        // StockUpdated queue yapılandırması
        cfg.ReceiveEndpoint(builder.Configuration["RabbitMQ:Queues:StockUpdated"], e =>
        {
            e.ConfigureConsumer<StockUpdatedEventConsumer>(context);
            e.UseMessageRetry(r => r.Interval(3, TimeSpan.FromSeconds(2)));
        });

        // Global hata yakalama
        cfg.UseMessageRetry(r =>
        {
            r.Incremental(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(2));
        });

        // Circuit breaker
        cfg.UseCircuitBreaker(cb =>
        {
            cb.TrackingPeriod = TimeSpan.FromMinutes(1);
            cb.TripThreshold = 15;
            cb.ActiveThreshold = 10;
            cb.ResetInterval = TimeSpan.FromMinutes(5);
        });
    });
});
builder.Services.AddHealthChecks()
    .AddRabbitMQ(
        async serviceProvider =>
        {
            var factory = new RabbitMQ.Client.ConnectionFactory
            {
                HostName = builder.Configuration["RabbitMQ:Host"],
                UserName = builder.Configuration["RabbitMQ:Username"],
                Password = builder.Configuration["RabbitMQ:Password"],
                VirtualHost = builder.Configuration["RabbitMQ:VirtualHost"] ?? "/",
                Port = builder.Configuration.GetValue<int>("RabbitMQ:Port", 5672)
            };
            return await factory.CreateConnectionAsync();
        },
        "RabbitMQ Health Check",
        tags: new[] { "rabbitmq", "messagebroker" },
        timeout: TimeSpan.FromSeconds(5)
    );

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer("Admin", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(5),
            ValidIssuer = builder.Configuration["Security:Token:Issuer"],
            ValidAudience = builder.Configuration["Security:Token:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Security:Token:SecurityKey"])),
            LifetimeValidator = (notBefore, expires, securityToken, validationParameters) => expires != null ? expires > DateTime.UtcNow : false,
            NameClaimType = ClaimTypes.Name
        };

        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                var accessToken = context.Request.Query["access_token"];
                var path = context.HttpContext.Request.Path;
                if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/order-hub"))
                {
                    context.Token = accessToken;
                }
                return Task.CompletedTask;
            },
            OnAuthenticationFailed = context =>
            {
                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Add("Token-Expired", "true");
                }
                return Task.CompletedTask;
            }
        };
    });

var app = builder.Build();

Log.Information("Starting web application");

using (var scope = app.Services.CreateScope())
{
    await RoleAndUserSeeder.SeedAsync(scope.ServiceProvider);
}

// Development specific middleware
if (app.Environment.IsDevelopment())
{
    builder.Configuration["UseAzureKeyVault"] = "false";
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    builder.Configuration["UseAzureKeyVault"] = "true";
    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{builder.Configuration["AzureKeyVault:VaultName"]}.vault.azure.net/"),
        new ClientSecretCredential(
            builder.Configuration["AzureKeyVault:TenantId"],
            builder.Configuration["AzureKeyVault:ClientId"],
            builder.Configuration["AzureKeyVault:ClientSecret"]));
    app.UseHsts();
}

// Metrics & Monitoring
app.UseMetricServer();

// CORS - Must be before other middleware that might generate responses
app.UseCors();

// Security Headers
app.UseMiddleware<SecurityHeadersMiddleware>();

// Security Protection
if (builder.Configuration.GetValue<bool>("Security:DDoSProtection:Enabled", true))
{
    app.UseMiddleware<DDoSProtectionMiddleware>();
}

if (builder.Configuration.GetValue<bool>("Security:RateLimiting:Enabled", true))
{
    app.UseMiddleware<RateLimitingMiddleware>();
}

// Monitoring
app.UseMiddleware<RequestTimingMiddleware>();
app.UseMiddleware<AdvancedMetricsMiddleware>();

// Basic Middleware
app.UseHttpsRedirection();
app.UseStaticFiles(new StaticFileOptions
{
    ContentTypeProvider = provider
});

// Authentication & Authorization
app.UseAuthentication();
app.UseAuthorization();

// Request Logging
app.UseSerilogRequestLogging(options =>
{
    options.MessageTemplate = 
        "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestHost", httpContext.Request.Host.Value);
        diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
        diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);
        
        if (httpContext.User?.Identity?.IsAuthenticated == true)
        {
            diagnosticContext.Set("UserName", httpContext.User.Identity.Name);
        }
    };
});

// API Routes
app.MapControllers();

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

app.MapHubs();

try
{
    Log.Information("Starting web application");
    await app.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}