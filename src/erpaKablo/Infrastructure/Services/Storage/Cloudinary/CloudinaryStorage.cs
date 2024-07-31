using Application.Storage.Cloudinary;
using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Domain;
using Infrastructure.Adapters.Image.Cloudinary;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Storage.Cloudinary;

public class CloudinaryStorage : ICloudinaryStorage
{
    private readonly CloudinaryDotNet.Cloudinary _cloudinary;
    
    public CloudinaryStorage(IConfiguration configuration)
    {
        var cloudinaryAccountSettings = configuration.GetSection("CloudinaryAccount").Get<CloudinarySettings>();

        var account = new Account(
            cloudinaryAccountSettings?.CloudName,
            cloudinaryAccountSettings?.ApiKeyName,
            cloudinaryAccountSettings?.ApiSecretName
        );

        _cloudinary = new CloudinaryDotNet.Cloudinary(account);
    }

    /*public async Task<List<(string fileName, string path, string containerName)>> UploadAsync(string category,
        string path, List<IFormFile> files)
    {
        var datas = new List<(string fileName, string path, string containerName)>();
        foreach (IFormFile file in files)
        {
            ImageUploadParams imageUploadParams = new()
            {
                File = new FileDescription(file.Value, stream: file.OpenReadStream()),
                UseFilename = true,
                UniqueFilename = false,
                Overwrite = false
            };
            
            
            
            imageUploadParams.Folder = category;
            imageUploadParams.File.Value = file.Value;

            ImageUploadResult imageUploadResult = await _cloudinary.UploadAsync(imageUploadParams);
            datas.Add((file.Value, imageUploadResult.Url.ToString(), path));
            
        }
        return datas;
        
    }*/


    public async Task<List<(string fileName, string path, string containerName)>> UploadFileToStorage(string category,
        string path, string fileName, MemoryStream fileStream)
    {
        var datas = new List<(string fileName, string path, string containerName)>();
        ImageUploadParams imageUploadParams = new()
        {
            File = new FileDescription(fileName, stream: fileStream),
            UseFilename = true,
            UniqueFilename = false,
            Overwrite = false
        };
        
        imageUploadParams.Folder = $"{category}/{path}";
        imageUploadParams.PublicId = Path.GetFileNameWithoutExtension(fileName);
        
        var uploadResult = await _cloudinary.UploadAsync(imageUploadParams);
        
        if (uploadResult.Error == null)
        {
            datas.Add((fileName, uploadResult.SecureUrl.ToString(), category));
        }
        
        return datas;
    }

    public async Task DeleteAsync(string path)
    {
        var publicId = GetPublicId(path);
        if (!string.IsNullOrEmpty(publicId))
        {
            var deletionParams = new DeletionParams(publicId)
            {
                ResourceType = ResourceType.Image
            };
            await _cloudinary.DestroyAsync(deletionParams);
        }
    }

    public async Task<List<T>?> GetFiles<T>(string employeeId) where T : ImageFile, new()
    {
        var searchExpression = $"folder:{employeeId}";
        return await SearchAndMapResources<T>(searchExpression, employeeId);
    }

    public async Task<List<T>> GetFiles<T>(string category, string path) where T : ImageFile, new()
    {
        var fullPath = $"{category}/{path}";
        var searchExpression = $"folder:{fullPath}";
        return await SearchAndMapResources<T>(searchExpression, category);
    }

    public async Task<List<string?>> GetFiles(string path, string category)
    {
        var fullPath = $"{category}/{path}";
        var searchExpression = $"folder:{fullPath}";
        var searchResult = _cloudinary.Search().Expression(searchExpression).Execute();

        return searchResult.Resources.Select(resource => resource.SecureUrl.ToString()).ToList();
    }

    private async Task<List<T>> SearchAndMapResources<T>(string searchExpression, string category) where T : ImageFile, new()
    {
        var searchResult =  _cloudinary.Search().Expression(searchExpression).Execute();

        return searchResult.Resources.Select(resource => new T
        {
            Name = Path.GetFileName(resource.PublicId),
            Path = resource.SecureUrl.ToString(),
            Category = category,
            Storage = "Cloudinary"
        }).ToList();
    }

    public bool HasFile(string path, string fileName)
    {
        var publicId = GetPublicId($"{path}/{fileName}");
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
            // Dosya bulunamadı veya bir hata oluştu
            return false;
        }
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