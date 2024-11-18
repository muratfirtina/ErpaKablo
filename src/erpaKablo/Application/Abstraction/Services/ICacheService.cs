namespace Application.Abstraction.Services;

public interface ICacheService
{
    Task<Dictionary<string, T>> GetManyAsync<T>(IEnumerable<string> keys);
    Task SetManyAsync<T>(Dictionary<string, T> keyValues, TimeSpan? expiry = null);
    Task<T> GetOrCreateAsync<T>(string key, Func<Task<T>> factory, TimeSpan? expiry = null);
    Task<bool> IncrementAsync(string key, int value = 1, TimeSpan? expiry = null);
    Task<int> GetCounterAsync(string key);
}