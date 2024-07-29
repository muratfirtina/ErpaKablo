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
    private readonly IConfiguration _configuration;
    private readonly IOptions<StorageSettings> _storageSettings;

    public StorageService(ILocalStorage localStorage, ICloudinaryStorage cloudinaryStorage,IFileNameService fileNameService, IGoogleStorage googleStorage, IConfiguration configuration, IOptions<StorageSettings> storageSettings)
    {
        _localStorage = localStorage;
        _cloudinaryStorage = cloudinaryStorage;
        _fileNameService = fileNameService;
        _googleStorage = googleStorage;
        _configuration = configuration;
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
        await _localStorage.DeleteAsync(path);
        await _cloudinaryStorage.DeleteAsync(path);
        //await _azureStorage.DeleteAsync(path);
        await _googleStorage.DeleteAsync(path);
    }
    
    public string GetStorageUrl()
    {
        return _storageSettings.Value.StorageProvider switch
        {
            "LocalStorage" => _storageSettings.Value.LocalStorageUrl,
            "GoogleStorage" => _storageSettings.Value.GoogleStorageUrl,
            _ => _storageSettings.Value.LocalStorageUrl // varsayılan olarak LocalStorage
        };
    }

    public async Task<List<T>?> GetFiles<T>(string productId) where T : ImageFile, new()
    {
        return await _localStorage.GetFiles<T>(productId);
    }

    /*public async Task<List<string>> GetFiles(string category,string path)
    {
        string newPath = await _fileNameService.PathRenameAsync(path);
        // Burada bir örnekleme yapılıyor; gerçekte, her bir storage'tan dosyaları birleştirebilirsiniz.
        return await _localStorage.GetFiles(category,newPath);
    }*/

    public bool HasFile(string path, string fileName)
    {
        // Bu örnekte local storage kontrol ediliyor, diğerleri de benzer şekilde kontrol edilebilir.
        return _localStorage.HasFile(path, fileName);
    }

    public string StorageName { get; } 

    
    
}
