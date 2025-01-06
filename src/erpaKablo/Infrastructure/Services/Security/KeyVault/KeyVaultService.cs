using Application.Abstraction.Services;
using Application.Abstraction.Services.Configurations;
using Application.Services;
using Azure;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Services.Security.KeyVault;

public class KeyVaultService : IKeyVaultService
{
    private readonly SecretClient? _secretClient;
    private readonly ICacheService _cache;
    private readonly ILogger<KeyVaultService> _logger;
    private readonly IConfiguration _configuration;
    private readonly ICacheEncryptionService _cacheEncryption;

    private static readonly HashSet<string> SensitiveKeys = new(StringComparer.OrdinalIgnoreCase)
    {
        "JwtSecurityKey",
        "JwtIssuer",
        "JwtAudience"
    };

    public KeyVaultService(
        IConfiguration configuration,
        ICacheService cache,
        ILogger<KeyVaultService> logger,
        ICacheEncryptionService cacheEncryption)
    {
        _configuration = configuration;
        _cache = cache;
        _logger = logger;
        _cacheEncryption = cacheEncryption;

        if (_configuration.GetValue<bool>("UseAzureKeyVault", true))
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
        
        var (success, cachedValue) = await _cache.TryGetValueAsync<string>(cacheKey);
        if (success)
        {
            _logger.LogDebug("Cache hit for secret: {SecretName}", secretName);
            if (SensitiveKeys.Contains(secretName))
            {
                return await _cacheEncryption.DecryptFromCache(cachedValue);
            }
            return cachedValue;
        }

        try
        {
            string value;
            if (_secretClient != null)
            {
                var secret = await _secretClient.GetSecretAsync(secretName);
                value = secret.Value.Value;
            }
            else
            {
                value = _configuration[secretName];
                if (string.IsNullOrEmpty(value))
                {
                    _logger.LogWarning("Secret not found in configuration: {SecretName}", secretName);
                    return string.Empty;
                }
            }

            if (SensitiveKeys.Contains(secretName))
            {
                var encryptedValue = await _cacheEncryption.EncryptForCache(value);
                await _cache.SetAsync(cacheKey, encryptedValue, TimeSpan.FromMinutes(5));
                return value;
            }

            await _cache.SetAsync(cacheKey, value, TimeSpan.FromMinutes(5));
            return value;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to retrieve secret: {SecretName}", secretName);
            throw;
        }
    }

    public async Task<Dictionary<string, string>> GetSecretsAsync(string[] secretNames)
    {
        var cacheKeys = secretNames.Select(name => $"KeyVault_{name}").ToArray();
        var cachedSecrets = await _cache.GetManyAsync<string>(cacheKeys);

        var results = new Dictionary<string, string>();
        for (int i = 0; i < secretNames.Length; i++)
        {
            var secretName = secretNames[i];
            var cacheKey = cacheKeys[i];

            try
            {
                if (cachedSecrets.TryGetValue(cacheKey, out string cachedValue))
                {
                    results[secretName] = cachedValue;
                    continue;
                }

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
            await _cache.RemoveAsync($"KeyVault_{secretName}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to set secret: {SecretName}", secretName);
            throw;
        }
    }

    public async Task SetSecretsAsync(Dictionary<string, string> secrets, bool recoverIfDeleted = false)
    {
        var tasks = secrets.Select(secret => 
            SetSecretAsync(secret.Key, secret.Value, recoverIfDeleted));
        await Task.WhenAll(tasks);
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
            await _cache.RemoveAsync($"KeyVault_{secretName}");
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
        var tasks = secretNames.Select(async secretName =>
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
        });

        await Task.WhenAll(tasks);
        return results;
    }
}