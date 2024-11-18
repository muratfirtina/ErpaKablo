using Application.Abstraction.Services;
using Application.Enums;
using Domain;
using Infrastructure.Services.Cache;
using Infrastructure.Services.Security.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog.Context;

namespace Infrastructure.Middleware.RateLimiting;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly ICacheService _cache;
    private readonly IMetricsService _metrics;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly RateLimitConfig _settings;
    private readonly SlidingWindowRateLimiter _rateLimiter;
    private readonly SemaphoreSlim _throttler;

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        ICacheService cache,
        IMetricsService metrics,
        IServiceScopeFactory serviceScopeFactory,
        IOptions<SecuritySettings> settings,
        SlidingWindowRateLimiter rateLimiter)
    {
        _next = next ?? throw new ArgumentNullException(nameof(next));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        _serviceScopeFactory = serviceScopeFactory ?? throw new ArgumentNullException(nameof(serviceScopeFactory));
        
        // Null check for settings
        var securitySettings = settings?.Value ?? throw new ArgumentNullException(nameof(settings));
        _settings = securitySettings.RateLimitConfig ?? throw new ArgumentNullException("RateLimitConfig is not configured");
        
        _rateLimiter = rateLimiter ?? throw new ArgumentNullException(nameof(rateLimiter));
        
        // Ensure MaxConcurrentRequests is set and valid
        if (_settings.MaxConcurrentRequests <= 0)
        {
            _settings.MaxConcurrentRequests = 100; // Default value
        }
        
        _throttler = new SemaphoreSlim(_settings.MaxConcurrentRequests);

        // Log the initialization
        _logger.LogInformation(
            "Rate limiting initialized with: {MaxRequests} requests per hour, {MaxConcurrent} concurrent requests",
            _settings.RequestsPerHour,
            _settings.MaxConcurrentRequests);
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!_settings.Enabled)
        {
            await _next(context);
            return;
        }

        if (!await _throttler.WaitAsync(TimeSpan.FromSeconds(5)))
        {
            await HandleThrottlingResponse(context);
            return;
        }

        try
        {
            var clientIp = GetClientIpAddress(context);
            var path = context.Request.Path;
            var key = GenerateRateLimitKey(context);

            var (isAllowed, currentCount, retryAfter) = 
                await _rateLimiter.CheckRateLimitAsync(key);

            using (LogContext.PushProperty("RateLimitInfo", new
            {
                ClientIP = clientIp,
                RequestCount = currentCount,
                MaxRequests = _settings.RequestsPerHour,
                Path = path,
                IsAllowed = isAllowed
            }))
            {
                if (!isAllowed)
                {
                    using var scope = _serviceScopeFactory.CreateScope();
                    var alertService = scope.ServiceProvider.GetRequiredService<IAlertService>();
                    var logService = scope.ServiceProvider.GetRequiredService<ILogService>();

                    await HandleRateLimitExceeded(
                        context, clientIp, path, currentCount, retryAfter, alertService, logService);
                    return;
                }

                _metrics.TrackActiveConnection("http", 1);
                await _next(context);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in rate limiting middleware");
            throw;
        }
        finally
        {
            _metrics.TrackActiveConnection("http", -1);
            _throttler.Release();
        }
    }

    private async Task HandleRateLimitExceeded(
        HttpContext context,
        string clientIp,
        PathString path,
        int currentCount,
        TimeSpan? retryAfter,
        IAlertService alertService,
        ILogService logService)
    {
        // Metrik kaydı
        _metrics.IncrementRateLimitHit(clientIp, path);

        // Güvenlik log kaydı
        await logService.CreateLogAsync(new SecurityLog
        {
            Timestamp = DateTime.UtcNow,
            Level = "Warning",
            EventType = "RateLimit",
            ClientIP = clientIp,
            Path = path.ToString(),
            Message = "Rate limit exceeded",
            RequestCount = currentCount,
            MaxRequests = _settings.RequestsPerHour,
            UserAgent = context.Request.Headers["User-Agent"].ToString(),
            UserName = context.User?.Identity?.Name
        });

        // Alert gönderimi
        await alertService.SendAlertAsync(
            AlertType.RateLimit,
            "Rate limit exceeded",
            new Dictionary<string, string>
            {
                ["clientIp"] = clientIp,
                ["path"] = path.ToString(),
                ["requestCount"] = currentCount.ToString(),
                ["maxRequests"] = _settings.RequestsPerHour.ToString()
            });

        // Response
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.ContentType = "application/json";

        var response = new
        {
            error = "Rate limit exceeded. Please try again later.",
            retryAfter = retryAfter?.TotalSeconds ?? _settings.WindowSizeInMinutes,
            details = new
            {
                currentCount,
                limit = _settings.RequestsPerHour,
                windowSize = _settings.WindowSizeInMinutes
            }
        };

        await context.Response.WriteAsJsonAsync(response);
    }

    private async Task HandleThrottlingResponse(HttpContext context)
    {
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.ContentType = "application/json";
        
        var response = new
        {
            error = "Server is busy. Please try again later.",
            retryAfter = 5
        };

        await context.Response.WriteAsJsonAsync(response);
    }

    private string GenerateRateLimitKey(HttpContext context)
    {
        var clientIp = GetClientIpAddress(context);
        var userId = context.User?.Identity?.Name ?? "anonymous";
        return $"rate_limit_{userId}_{clientIp}_{DateTime.UtcNow:yyyyMMddHH}";
    }

    private string GetClientIpAddress(HttpContext context)
    {
        return context.Request.Headers["X-Forwarded-For"].FirstOrDefault() ??
               context.Connection.RemoteIpAddress?.ToString() ??
               "unknown";
    }
}