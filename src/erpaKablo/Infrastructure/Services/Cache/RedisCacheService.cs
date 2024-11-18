using Application.Abstraction.Services;
using Application.Abstraction.Services.Configurations;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Infrastructure.Services.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDistributedCache _distributedCache;
    private readonly IDatabase _redisDb;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly IMetricsService _metrics;
    private readonly ConnectionMultiplexer _redis;

    public RedisCacheService(
        IDistributedCache distributedCache,
        IConfiguration configuration,
        ILogger<RedisCacheService> logger,
        IMetricsService metrics,
        IKeyVaultService keyVaultService)
    {
        _distributedCache = distributedCache;
        _logger = logger;
        _metrics = metrics;

        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        
        // Redis bağlantı string'ini ortama göre al
        string redisConnection;
        if (environment.Equals("Production", StringComparison.OrdinalIgnoreCase))
        {
            // Production ortamında KeyVault'tan al
            redisConnection = keyVaultService.GetSecretAsync("RedisConnection").Result;
            if (string.IsNullOrEmpty(redisConnection))
            {
                throw new InvalidOperationException("Redis connection string not found in KeyVault");
            }
        }
        else
        {
            // Development ortamında configuration'dan al
            redisConnection = configuration.GetConnectionString("Redis");
            if (string.IsNullOrEmpty(redisConnection))
            {
                throw new InvalidOperationException("Redis connection string not found in configuration");
            }
        }

        _logger.LogInformation("Connecting to Redis in {Environment} environment", environment);
        _redis = ConnectionMultiplexer.Connect(redisConnection);
        _redisDb = _redis.GetDatabase();
    }

    public async Task<Dictionary<string, T>> GetManyAsync<T>(IEnumerable<string> keys)
    {
        try
        {
            var batch = _redisDb.CreateBatch();
            var tasks = keys.Select(key => batch.StringGetAsync(key)).ToList();
            batch.Execute();

            var results = await Task.WhenAll(tasks);
            var response = keys.Zip(results, (key, value) => new { key, value })
                .Where(x => !x.value.IsNull)
                .ToDictionary(
                    x => x.key,
                    x => JsonConvert.DeserializeObject<T>(x.value)
                );

            foreach (var key in response.Keys)
            {
                _metrics.IncrementCacheHit(key);
            }

            foreach (var key in keys.Except(response.Keys))
            {
                _metrics.IncrementCacheMiss(key);
            }

            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetManyAsync for keys: {Keys}", string.Join(", ", keys));
            throw;
        }
    }

    public async Task SetManyAsync<T>(Dictionary<string, T> keyValues, TimeSpan? expiry = null)
    {
        try
        {
            var batch = _redisDb.CreateBatch();
            var tasks = keyValues.Select(kv =>
                batch.StringSetAsync(
                    kv.Key,
                    JsonConvert.SerializeObject(kv.Value),
                    expiry
                )
            ).ToList();

            batch.Execute();
            await Task.WhenAll(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SetManyAsync for keys: {Keys}", 
                string.Join(", ", keyValues.Keys));
            throw;
        }
    }

    public async Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null)
    {
        try
        {
            var value = await _redisDb.StringGetAsync(key);
            if (!value.IsNull)
            {
                _metrics.IncrementCacheHit(key);
                return JsonConvert.DeserializeObject<T>(value);
            }

            _metrics.IncrementCacheMiss(key);
            var newValue = await factory();

            await _redisDb.StringSetAsync(
                key,
                JsonConvert.SerializeObject(newValue),
                expiry ?? TimeSpan.FromHours(1)
            );

            return newValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in GetOrCreateAsync for key: {Key}", key);
            return await factory();
        }
    }

    public async Task<bool> IncrementAsync(string key, int value = 1, TimeSpan? expiry = null)
    {
        try
        {
            var result = await _redisDb.StringIncrementAsync(key, value);
            if (expiry.HasValue)
            {
                await _redisDb.KeyExpireAsync(key, expiry.Value);
            }
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing key: {Key}", key);
            return false;
        }
    }

    public async Task<int> GetCounterAsync(string key)
    {
        try
        {
            var value = await _redisDb.StringGetAsync(key);
            return value.IsNull ? 0 : (int)value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting counter for key: {Key}", key);
            return 0;
        }
    }
}