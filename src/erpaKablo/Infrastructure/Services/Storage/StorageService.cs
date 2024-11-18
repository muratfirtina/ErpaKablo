using Application.Services;
using Application.Storage;
using Application.Storage.Cloudinary;
using Application.Storage.Google;
using Application.Storage.Local;
using Domain;
using Infrastructure.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Storage;

public class StorageService : IStorageService
{
    private readonly ILocalStorage _localStorage;
    private readonly ICloudinaryStorage _cloudinaryStorage;
    private readonly IGoogleStorage _googleStorage;
    private readonly IFileNameService _fileNameService;
    private readonly IOptionsSnapshot<StorageSettings> _storageSettings;

    public StorageService(
        ILocalStorage localStorage,
        ICloudinaryStorage cloudinaryStorage,
        IGoogleStorage googleStorage,
        IFileNameService fileNameService,
        IOptionsSnapshot<StorageSettings> storageSettings)
    {
        _localStorage = localStorage ?? throw new ArgumentNullException(nameof(localStorage));
        _cloudinaryStorage = cloudinaryStorage ?? throw new ArgumentNullException(nameof(cloudinaryStorage));
        _googleStorage = googleStorage ?? throw new ArgumentNullException(nameof(googleStorage));
        _fileNameService = fileNameService ?? throw new ArgumentNullException(nameof(fileNameService));
        _storageSettings = storageSettings ?? throw new ArgumentNullException(nameof(storageSettings));
    }


    public async Task<List<(string fileName, string path, string entityType, string storageType)>> UploadAsync(string entityType, string path, List<IFormFile> files)
    {
        List<(string fileName, string path, string entityType, string storageType)> datas = new List<(string fileName, string path, string entityType, string storageType)>();
        foreach (var file in files)
        {
            await _fileNameService.FileMustBeInFileFormat(file);
            string storageType = StorageType.Local.ToString();
            string newPath = await _fileNameService.PathRenameAsync(path);
            var fileNewName = await _fileNameService.FileRenameAsync(newPath, file.FileName, (p, f) => HasFile(entityType, p, f));
            
            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            memoryStream.Position = 0;
            await UploadToStorage(entityType, newPath, fileNewName, memoryStream);
            datas.Add((fileNewName, newPath, entityType, storageType));

            memoryStream.Close();
        }

        return datas;
    }

    private async Task UploadToStorage(string entityType, string path, string fileName, MemoryStream fileStream)
    {
        fileStream.Position = 0;
        await _localStorage.UploadFileToStorage(entityType, path, fileName, new MemoryStream(fileStream.ToArray()));

        fileStream.Position = 0;
        await _cloudinaryStorage.UploadFileToStorage(entityType, path, fileName, new MemoryStream(fileStream.ToArray()));
        
        /*fileStream.Position = 0;
        await _googleStorage.UploadFileToStorage(entityType, path, fileName, new MemoryStream(fileStream.ToArray()));*/
    }
    
    public async Task<List<T>?> GetFiles<T>(string entityId, string entityType, string preferredStorage = null) where T : ImageFile, new()
    {
        var activeProvider = preferredStorage ?? _storageSettings.Value.ActiveProvider;

        switch (activeProvider.ToLower())
        {
            case "localstorage":
                return await _localStorage.GetFiles<T>(entityId, entityType);
            case "cloudinary":
                return await _cloudinaryStorage.GetFiles<T>(entityId, entityType);
            /*case "google":
                return await _googleStorage.GetFiles<T>(entityId, entityType);*/
            default:
                throw new ArgumentException("Invalid storage provider", nameof(preferredStorage));
        }
    }

    public string GetStorageUrl(string storageType = null)
    {
        if (_storageSettings?.Value?.Providers == null)
            throw new InvalidOperationException("Storage settings or providers are not configured");

        var activeProvider = (storageType ?? _storageSettings.Value.ActiveProvider ?? "localstorage").ToLower();
        
        return activeProvider switch
        {
            "localstorage" => _storageSettings.Value.Providers.LocalStorage?.Url 
                              ?? throw new InvalidOperationException("LocalStorage URL is not configured"),
            "cloudinary" => _storageSettings.Value.Providers.Cloudinary?.Url 
                            ?? throw new InvalidOperationException("Cloudinary URL is not configured"),
            /*"google" => _storageSettings.Value.Providers.Google?.Url 
                        ?? throw new InvalidOperationException("Google Storage URL is not configured"),*/
            _ => throw new ArgumentException($"Invalid storage provider: {activeProvider}", nameof(storageType))
        };
    }

    public bool HasFile(string entityType, string path, string fileName)
    {
        return _localStorage.HasFile(entityType, path, fileName);
    }

    public string StorageName { get; } 

    
    public async Task DeleteFromAllStoragesAsync(string entityType, string path, string fileName)
    {
        try
        {
            await _localStorage.DeleteAsync(entityType, path, fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting from local storage: {ex.Message}");
        }

        try
        {
            await _cloudinaryStorage.DeleteAsync(entityType, path, fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting from Cloudinary: {ex.Message}");
        }

        /*try
        {
            await _googleStorage.DeleteAsync(entityType, path, fileName);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting from Google Storage: {ex.Message}");
        }*/
    }
}
