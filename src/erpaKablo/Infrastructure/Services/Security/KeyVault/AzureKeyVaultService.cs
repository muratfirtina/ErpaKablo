using System.Security;
using Application.Abstraction.Services.Configurations;
using Azure;
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
            _logger.LogDebug("Cache hit for secret: {SecretName}", secretName);
            return cachedValue;
        }

        try
        {
            if (_secretClient != null)
            {
                var secret = await _secretClient.GetSecretAsync(secretName);
                var value = secret.Value.Value;
                _cache.Set(cacheKey, value, TimeSpan.FromMinutes(CacheTimeInMinutes));
                return value;
            }

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

    public async Task<Dictionary<string, string>> GetSecretsAsync(string[] secretNames)
    {
        var results = new Dictionary<string, string>();
        foreach (var secretName in secretNames)
        {
            try
            {
                var value = await GetSecretAsync(secretName);
                results[secretName] = value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve secret: {SecretName}", secretName);
                results[secretName] = string.Empty;
            }
        }
        return results;
    }

    public async Task SetSecretAsync(string secretName, string value, bool recoverIfDeleted = false)
    {
        try
        {
            if (_secretClient != null)
            {
                if (recoverIfDeleted)
                {
                    try
                    {
                        var deletedSecret = await _secretClient.GetDeletedSecretAsync(secretName);
                        if (deletedSecret.Value != null)
                        {
                            _logger.LogInformation("Recovering deleted secret: {SecretName}", secretName);
                            var recoverOperation = await _secretClient.StartRecoverDeletedSecretAsync(secretName);
                            await recoverOperation.WaitForCompletionAsync();
                        }
                    }
                    catch (RequestFailedException ex) when (ex.Status == 404)
                    {
                        _logger.LogDebug("No deleted secret found to recover: {SecretName}", secretName);
                    }
                }

                await _secretClient.SetSecretAsync(secretName, value);
                _logger.LogInformation("Secret set successfully: {SecretName}", secretName);
            }
            _cache.Remove($"KeyVault_{secretName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set secret: {SecretName}", secretName);
            throw;
        }
    }

    public async Task SetSecretsAsync(Dictionary<string, string> secrets, bool recoverIfDeleted = false)
    {
        foreach (var secret in secrets)
        {
            await SetSecretAsync(secret.Key, secret.Value, recoverIfDeleted);
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
                    try
                    {
                        var secret = await _secretClient.GetSecretAsync(secretProperty.Name);
                        secrets.Add(secretProperty.Name, secret.Value.Value);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to retrieve secret: {SecretName}", secretProperty.Name);
                    }
                }
            }
            else
            {
                foreach (var config in _configuration.AsEnumerable().Where(x => !string.IsNullOrEmpty(x.Value)))
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

    public async Task DeleteSecretAsync(string secretName)
    {
        try
        {
            if (_secretClient != null)
            {
                await _secretClient.StartDeleteSecretAsync(secretName);
                _logger.LogInformation("Secret deleted successfully: {SecretName}", secretName);
            }
            _cache.Remove($"KeyVault_{secretName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete secret: {SecretName}", secretName);
            throw;
        }
    }

    public async Task<Dictionary<string, bool>> DeleteSecretsAsync(string[] secretNames)
    {
        var results = new Dictionary<string, bool>();
        foreach (var secretName in secretNames)
        {
            try
            {
                await DeleteSecretAsync(secretName);
                results[secretName] = true;
            }
            catch (Exception)
            {
                results[secretName] = false;
            }
        }
        return results;
    }
}