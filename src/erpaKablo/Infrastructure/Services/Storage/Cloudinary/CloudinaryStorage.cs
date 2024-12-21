using Application.Storage.Cloudinary;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain;
using Infrastructure.Adapters.Image.Cloudinary;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Storage.Cloudinary;

public class CloudinaryStorage : ICloudinaryStorage
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;
    private readonly IOptionsSnapshot<StorageSettings> _storageSettings;
    
    public CloudinaryStorage(IConfiguration configuration, IOptionsSnapshot<StorageSettings> storageSettings)
    {
        _storageSettings = storageSettings;
        
        // Storage ayarlarından Cloudinary yapılandırmasını al
        var cloudinarySettings = configuration
            .GetSection("Storage:Providers:Cloudinary")
            .Get<CloudinarySettings>();

        if (cloudinarySettings == null)
        {
            throw new InvalidOperationException("Cloudinary settings are not properly configured in appsettings.json");
        }

        var account = new Account(
            cloudinarySettings.CloudName,
            cloudinarySettings.ApiKey,
            cloudinarySettings.ApiSecret
        );

        _cloudinary = new CloudinaryDotNet.Cloudinary(account);
    }

    public async Task<List<(string fileName, string path, string containerName, string url, string format)>> UploadFileToStorage(
        string entityType, 
        string path, 
        string fileName, 
        MemoryStream fileStream)
    {
        var datas = new List<(string fileName, string path, string containerName, string url, string format)>();
    
        ImageUploadParams imageUploadParams = new()
        {
            File = new FileDescription(fileName, stream: fileStream),
            UseFilename = true,
            UniqueFilename = false,
            Overwrite = false
        };
    
        imageUploadParams.Folder = $"{entityType}/{path}";
        imageUploadParams.PublicId = Path.GetFileNameWithoutExtension(fileName);
    
        var uploadResult = await _cloudinary.UploadAsync(imageUploadParams);
    
        if (uploadResult.Error == null)
        {
            var format = Path.GetExtension(fileName).TrimStart('.').ToLower();
            datas.Add((fileName, path, entityType, uploadResult.SecureUrl.ToString(), format));
        }
    
        return datas;
    }
    public async Task DeleteAsync(string entityType, string path, string fileName)
    {
        var publicId = GetPublicId($"{entityType}/{path}/{fileName}");
        if (!string.IsNullOrEmpty(publicId))
        {
            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };
            await _cloudinary.DestroyAsync(deletionParams);
        }
    }

    public async Task<List<T>?> GetFiles<T>(string entityId, string entityType) where T : ImageFile, new()
    {
        var baseUrl = _storageSettings.Value.Providers.Cloudinary.Url;
        var searchExpression = $"folder:{entityType}/{entityId}";
        var searchResult = _cloudinary.Search().Expression(searchExpression).Execute();

        return searchResult.Resources.Select(resource => new T
        {
            Id = Path.GetFileNameWithoutExtension(resource.PublicId),
            Name = Path.GetFileName(resource.PublicId),
            Path = Path.GetDirectoryName(resource.PublicId),
            EntityType = entityType,
            Storage = "Cloudinary",
            Url = resource.SecureUrl.ToString()
        }).ToList();
    }

    public bool HasFile(string entityType, string path, string fileName)
    {
        var publicId = GetPublicId($"{entityType}/{path}/{fileName}");
        var getResourceParams = new GetResourceParams(publicId)
        {
            ResourceType = ResourceType.Image
        };

        try
        {
            var result = _cloudinary.GetResource(getResourceParams);
            return result != null && !string.IsNullOrEmpty(result.PublicId);
        }
        catch (Exception)
        {
            return false;
        }
    }

    public string GetStorageUrl()
    {
        return _storageSettings.Value.Providers.Cloudinary.Url ?? 
               throw new InvalidOperationException("Cloudinary URL is not configured");
    }

    private string GetPublicId(string fullPath)
    {
        var pathParts = fullPath.Split('/');
        if (pathParts.Length >= 3)
        {
            var fileName = pathParts[pathParts.Length - 1];
            var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);
            
            return string.Join("/", pathParts.Take(pathParts.Length - 1).Append(fileNameWithoutExtension));
        }
        return string.Empty;
    }
}