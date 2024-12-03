using System.Security.Claims;
using System.Text;
using Application;
using Application.Extensions;
using Azure.Identity;
using Infrastructure;
using Infrastructure.Middleware.DDosProtection;
using Infrastructure.Middleware.Monitoring;
using Infrastructure.Middleware.RateLimiting;
using Infrastructure.Middleware.Security;
using Infrastructure.Services.Monitoring;
using Infrastructure.Services.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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