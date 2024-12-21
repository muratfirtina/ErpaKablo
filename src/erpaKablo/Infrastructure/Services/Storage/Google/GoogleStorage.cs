using Application.Storage.Google;
using Domain;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Storage.Google;

public class GoogleStorage : IGoogleStorage
{
    private readonly StorageClient _storageClient;
    private readonly IOptionsSnapshot<StorageSettings> _storageSettings;
    private readonly string _baseUrl;
    private readonly string _bucketName = "tumdeximages";


    public GoogleStorage(IConfiguration configuration, IOptionsSnapshot<StorageSettings> storageSettings)
    {
        _storageSettings = storageSettings ?? throw new ArgumentNullException(nameof(storageSettings));
        
        var googleSettings = configuration.GetSection("Storage:Providers:Google").Get<GoogleStorageSettings>();
        if (googleSettings == null)
        {
            throw new InvalidOperationException("Google Storage settings are not properly configured in appsettings.json");
        }

        _baseUrl = googleSettings.Url ?? throw new InvalidOperationException("Google Storage URL is not configured");

        // Credential dosyasının yolunu kontrol et
        var credentialsPath = googleSettings.CredentialsFilePath;
        if (string.IsNullOrEmpty(credentialsPath))
        {
            throw new InvalidOperationException("Google Storage credentials file path is not configured");
        }

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

    // Implement your storage methods here
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
        var baseUrl = _storageSettings.Value.Providers.Google.Url;
        var prefix = $"{entityType}/{entityId}/";
        var objects = _storageClient.ListObjects(_bucketName, prefix);

        List<T> files = new List<T>();

        foreach (var obj in objects)
        {
            var file = new T
            {
                Id = Path.GetFileNameWithoutExtension(obj.Name),
                Name = Path.GetFileName(obj.Name),
                Path = Path.GetDirectoryName(obj.Name),
                EntityType = entityType,
                Storage = "Google",
                Url = $"{baseUrl}/{obj.Name}"
            };
            files.Add(file);
        }

        return files;
    }

    public bool HasFile(string entityType, string path, string fileName)
    {
        try
        {
            var bucketName = _bucketName;
            var objectName = $"{entityType}/{path}/{fileName}";

            var obj = _storageClient.GetObject(bucketName, objectName, new GetObjectOptions { Projection = Projection.NoAcl });
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

public class GoogleStorageSettings
{
    public string Url { get; set; }
    public string CredentialsFilePath { get; set; }
}