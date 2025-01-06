using Application.Storage.Google;
using Domain;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Infrastructure.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Storage.Google;

public class GoogleStorage : IGoogleStorage
{
    private readonly StorageClient _storageClient;
    private readonly IOptionsSnapshot<StorageSettings> _storageSettings;
    private readonly string _baseUrl;
    private readonly string _bucketName;
    private readonly IConfiguration _configuration;

    public GoogleStorage(
        IConfiguration configuration, 
        IOptionsSnapshot<StorageSettings> storageSettings)
    {
        _configuration = configuration;
        _storageSettings = storageSettings ?? throw new ArgumentNullException(nameof(storageSettings));
        
        // Key Vault'tan Google Storage ayarlarını al
        _bucketName = configuration.GetSecretFromKeyVault("GoogleStorageBucketName") ?? 
                      throw new InvalidOperationException("Google Storage bucket name not found in Key Vault");
            
        _baseUrl = configuration.GetSecretFromKeyVault("GoogleStorageUrl") ?? 
                   throw new InvalidOperationException("Google Storage URL not found in Key Vault");
            
        var credentialsPath = configuration.GetSecretFromKeyVault("GoogleStorageCredentialsPath") ?? 
                              throw new InvalidOperationException("Google Storage credentials path not found in Key Vault");

        if (!File.Exists(credentialsPath))
        {
            throw new FileNotFoundException($"Google credentials file not found at: {credentialsPath}");
        }

        try
        {
            var credential = GoogleCredential.FromFile(credentialsPath);
            _storageClient = StorageClient.Create(credential);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to initialize Google Storage client: {ex.Message}", ex);
        }
    }
    

    public async Task<List<(string fileName, string path, string containerName, string url, string format)>> UploadFileToStorage(
        string entityType, 
        string path, 
        string fileName, 
        MemoryStream fileStream)
    {
        var results = new List<(string fileName, string path, string containerName, string url, string format)>();
        try
        {
            var objectName = $"{entityType}/{path}/{fileName}";
            await _storageClient.UploadObjectAsync(_bucketName, objectName, null, fileStream);
        
            var format = Path.GetExtension(fileName).TrimStart('.').ToLower();
            var url = $"{_baseUrl}/{objectName}";
            results.Add((fileName, path, entityType, url, format));
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to upload file to Google Storage: {ex.Message}", ex);
        }

        return results;
    }

    public async Task DeleteAsync(string entityType, string path, string fileName)
    {
        try
        {
            var objectName = $"{entityType}/{path}/{fileName}";
            await _storageClient.DeleteObjectAsync(_bucketName, objectName);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to delete file from Google Storage: {ex.Message}", ex);
        }
    }

    public async Task<List<T>?> GetFiles<T>(string entityId, string entityType) where T : ImageFile, new()
    {
        try
        {
            var prefix = $"{entityType}/{entityId}/";
            var objects = _storageClient.ListObjects(_bucketName, prefix);

            return objects.Select(obj => new T
            {
                Id = Path.GetFileNameWithoutExtension(obj.Name),
                Name = Path.GetFileName(obj.Name),
                Path = Path.GetDirectoryName(obj.Name),
                EntityType = entityType,
                Storage = "Google",
                Url = $"{_baseUrl}/{obj.Name}"
            }).ToList();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"Failed to get files from Google Storage: {ex.Message}", ex);
        }
    }

    public bool HasFile(string entityType, string path, string fileName)
    {
        try
        {
            var objectName = $"{entityType}/{path}/{fileName}";
            var obj = _storageClient.GetObject(_bucketName, objectName, new GetObjectOptions 
            { 
                Projection = Projection.NoAcl 
            });
            return obj != null;
        }
        catch
        {
            return false;
        }
    }

    public string GetStorageUrl()
    {
        return _storageSettings.Value.Providers.Google.Url ?? 
               throw new InvalidOperationException("Google Storage URL is not configured");
    }
}
