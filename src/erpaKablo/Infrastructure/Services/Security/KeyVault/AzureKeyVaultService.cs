using System.Security;
using Application.Abstraction.Services.Configurations;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Security.KeyVault;

public class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient? _secretClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<KeyVaultService> _logger;
    private readonly IConfiguration _configuration;
    private const int CacheTimeInMinutes = 5;

    public KeyVaultService(
        IConfiguration configuration,
        IMemoryCache cache,
        ILogger<KeyVaultService> logger)
    {
        _configuration = configuration;
        _cache = cache;
        _logger = logger;

        // Sadece production ortamında Azure Key Vault'u başlat
        if (_configuration.GetValue<bool>("UseAzureKeyVault", false))
        {
            var keyVaultUrl = _configuration["AzureKeyVault:VaultUri"];
            if (!string.IsNullOrEmpty(keyVaultUrl))
            {
                var credential = new DefaultAzureCredential();
                _secretClient = new SecretClient(new Uri(keyVaultUrl), credential);
            }
        }
    }

    public async Task<string> GetSecretAsync(string secretName)
    {
        var cacheKey = $"KeyVault_{secretName}";
        
        if (_cache.TryGetValue(cacheKey, out string cachedValue))
        {
            return cachedValue;
        }

        try
        {
            // Production ortamında Azure Key Vault'tan al
            if (_secretClient != null)
            {
                var secret = await _secretClient.GetSecretAsync(secretName);
                var value = secret.Value.Value;
                _cache.Set(cacheKey, value, TimeSpan.FromMinutes(CacheTimeInMinutes));
                return value;
            }

            // Development ortamında appsettings.json'dan al
            var configValue = _configuration[secretName];
            if (string.IsNullOrEmpty(configValue))
            {
                _logger.LogWarning("Secret not found in configuration: {SecretName}", secretName);
                return string.Empty;
            }

            _cache.Set(cacheKey, configValue, TimeSpan.FromMinutes(CacheTimeInMinutes));
            return configValue;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secret: {SecretName}", secretName);
            throw;
        }
    }

    public async Task SetSecretAsync(string secretName, string value)
    {
        try
        {
            if (_secretClient != null)
            {
                await _secretClient.SetSecretAsync(secretName, value);
            }
            _cache.Remove($"KeyVault_{secretName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set secret: {SecretName}", secretName);
            throw;
        }
    }

    public async Task<Dictionary<string, string>> GetAllSecretsAsync()
    {
        var secrets = new Dictionary<string, string>();
        
        try
        {
            if (_secretClient != null)
            {
                var allSecrets = _secretClient.GetPropertiesOfSecretsAsync();
                await foreach (var secretProperty in allSecrets)
                {
                    var secret = await _secretClient.GetSecretAsync(secretProperty.Name);
                    secrets.Add(secretProperty.Name, secret.Value.Value);
                }
            }
            else
            {
                // Development ortamında tüm configuration'ı döndür
                foreach (var config in _configuration.AsEnumerable())
                {
                    secrets.Add(config.Key, config.Value);
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve all secrets");
            throw;
        }

        return secrets;
    }
}