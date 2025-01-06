using Application.Abstraction.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace Infrastructure.Services.Cache;

public class RedisCacheService : ICacheService
{
    private readonly IDatabase _redisDb;
    private readonly ILogger<RedisCacheService> _logger;
    private readonly IMetricsService _metrics;

    public RedisCacheService(
        IConnectionMultiplexer connectionMultiplexer,
        ILogger<RedisCacheService> logger,
        IMetricsService metrics)
    {
        _redisDb = connectionMultiplexer.GetDatabase() ?? throw new ArgumentNullException(nameof(connectionMultiplexer));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _metrics = metrics ?? throw new ArgumentNullException(nameof(metrics));
        
        _logger.LogInformation("Initializing Redis Cache Service");
    }

    public async Task<(bool success, T value)> TryGetValueAsync<T>(string key)
    {
        try
        {
            var redisValue = await _redisDb.StringGetAsync(key);
            if (!redisValue.IsNull)
            {
                _metrics?.IncrementCacheHit(key);
                var value = JsonConvert.DeserializeObject<T>(redisValue);
                return (true, value);
            }

            _metrics?.IncrementCacheMiss(key);
            return (false, default);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in TryGetValueAsync for key: {Key}", key);
            return (false, default);
        }
    }

    public async Task SetAsync<T>(string key, T value, TimeSpan expiration)
    {
        try
        {
            var serializedValue = JsonConvert.SerializeObject(value);
            await _redisDb.StringSetAsync(key, serializedValue, expiration);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in SetAsync for key: {Key}", key);
            throw;
        }
    }

    public async Task<bool> RemoveAsync(string key)
    {
        try
        {
            return await _redisDb.KeyDeleteAsync(key);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in RemoveAsync for key: {Key}", key);
            throw;
        }
    }

    public async Task<Dictionary<string, T>> GetManyAsync<T>(IEnumerable<string> keys)
    {
        try
        {
            var batch = _redisDb.CreateBatch();
            var tasks = keys.Select(key => batch.StringGetAsync(key)).ToList();
            batch.Execute();

            var results = await Task.WhenAll(tasks);
            var response = new Dictionary<string, T>();

            foreach (var pair in keys.Zip(results, (key, value) => new { key, value }))
            {
                if (!pair.value.IsNull)
                {
                    try
                    {
                        var deserializedValue = JsonConvert.DeserializeObject<T>(pair.value);
                        response[pair.key] = deserializedValue;
                        _metrics.IncrementCacheHit(pair.key);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to deserialize value for key: {Key}", pair.key);
                        _metrics.IncrementCacheMiss(pair.key);
                    }
                }
                else
                {
                    _metrics.IncrementCacheMiss(pair.key);
                }
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
            var result = await TryGetValueAsync<T>(key);
            if (result.success)
            {
                return result.value;
            }

            var newValue = await factory();
            await SetAsync(key, newValue, expiry ?? TimeSpan.FromHours(1));
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