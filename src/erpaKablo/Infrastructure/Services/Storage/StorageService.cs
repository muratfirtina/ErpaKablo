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
        _localStorage = localStorage;
        _cloudinaryStorage = cloudinaryStorage;
        _googleStorage = googleStorage;
        _fileNameService = fileNameService;
        _storageSettings = storageSettings;
    }


    public async Task<List<(string fileName, string path, string category,string storageType)>> UploadAsync(string category, string path,
        List<IFormFile> files)
    {
        List<(string fileName, string path, string category,string storageType)> datas = new List<(string fileName, string path, string category,string storageType)>();
        foreach (var file in files)
        {

            await _fileNameService.FileMustBeInFileFormat(file);
            string storageType = StorageType.Local.ToString();
            string newPath = await _fileNameService.PathRenameAsync(path);
            var fileNewName = await _fileNameService.FileRenameAsync(newPath, file.FileName,HasFile);
            
            var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            // Dosyayı her bir storage'a yükle
            memoryStream.Position = 0; // Akışın başına dön
            
            await UploadToStorage(category, newPath, fileNewName, memoryStream);
            datas.Add((fileNewName, newPath, category, storageType));

            memoryStream.Close(); // MemoryStream'i manuel olarak kapat
        }

        return datas;
    }

    private async Task UploadToStorage(string category, string path, string fileName, MemoryStream fileStream)
    {
        // MemoryStream'in pozisyonunu sıfırla
        fileStream.Position = 0;
        await _localStorage.UploadFileToStorage(category, path, fileName, new MemoryStream(fileStream.ToArray()));

        fileStream.Position = 0;
        await _cloudinaryStorage.UploadFileToStorage(category, path, fileName, new MemoryStream(fileStream.ToArray()));
        
        fileStream.Position = 0;
        await _googleStorage.UploadFileToStorage(category, path, fileName, new MemoryStream(fileStream.ToArray()));
        
    }

    public async Task DeleteAsync(string path)
    {
        var pathParts = path.Split('/');
        if (pathParts.Length >= 3)
        {
            var category = pathParts[0];
            var fileName = pathParts[pathParts.Length - 1];
            var filePath = string.Join("/", pathParts.Skip(1).Take(pathParts.Length - 2));
            await DeleteFromAllStoragesAsync(category, filePath, fileName);
        }
        else
        {
            throw new ArgumentException("Invalid path format", nameof(path));
        }
    }
    

    public async Task<List<T>?> GetFiles<T>(string productId, string preferredStorage = null) where T : ImageFile, new()
    {
        var activeProvider = preferredStorage ?? _storageSettings.Value.ActiveProvider;

        switch (activeProvider.ToLower())
        {
            case "localstorage":
                return await _localStorage.GetFiles<T>(productId);
            case "cloudinary":
                return await _cloudinaryStorage.GetFiles<T>(productId);
            case "google":
                return await _googleStorage.GetFiles<T>(productId);
            default:
                throw new ArgumentException("Invalid storage provider", nameof(preferredStorage));
        }
    }

    public string GetStorageUrl(string storageType = null)
    {
        var activeProvider = storageType ?? _storageSettings.Value.ActiveProvider;
        return activeProvider.ToLower() switch
        {
            "localstorage" => _storageSettings.Value.Providers.LocalStorage.Url,
            "cloudinary" => _storageSettings.Value.Providers.Cloudinary.Url,
            "google" => _storageSettings.Value.Providers.Google.Url,
            _ => throw new ArgumentException("Invalid storage provider", nameof(storageType))
        };
    }

    public bool HasFile(string path, string fileName)
    {
        // Bu örnekte local storage kontrol ediliyor, diğerleri de benzer şekilde kontrol edilebilir.
        return _localStorage.HasFile(path, fileName);
    }

    public string StorageName { get; } 

    
    public async Task DeleteFromAllStoragesAsync(string category, string path, string fileName)
    {
        try
        {
            await _localStorage.DeleteAsync($"{category}/{path}/{fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting from local storage: {ex.Message}");
        }

        try
        {
            await _cloudinaryStorage.DeleteAsync($"{category}/{path}/{fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting from Cloudinary: {ex.Message}");
        }

        try
        {
            await _googleStorage.DeleteAsync($"{category}/{path}/{fileName}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error deleting from Google Storage: {ex.Message}");
        }
    }
}
