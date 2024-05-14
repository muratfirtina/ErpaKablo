using Application.Services;
using Application.Storage;
using Application.Storage.Cloudinary;
using Application.Storage.Local;
using Domain;
using Infrastructure.Enums;
using Microsoft.AspNetCore.Http;

namespace Infrastructure.Services.Storage;

public class StorageService : IStorageService
{
    private readonly ILocalStorage _localStorage;
    private readonly ICloudinaryStorage _cloudinaryStorage;
    private readonly IFileNameService _fileNameService;

    public StorageService(ILocalStorage localStorage, ICloudinaryStorage cloudinaryStorage,IFileNameService fileNameService)
    {
        _localStorage = localStorage;
        _cloudinaryStorage = cloudinaryStorage;
        _fileNameService = fileNameService;
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
        
    }

    public async Task DeleteAsync(string path)
    {
        await _localStorage.DeleteAsync(path);
        await _cloudinaryStorage.DeleteAsync(path);
        //await _azureStorage.DeleteAsync(path);
        //await _googleStorage.DeleteAsync(path);
    }

    public async Task<List<T>?> GetFiles<T>(string employeeId) where T : ImageFile, new()
    {
        return await _localStorage.GetFiles<T>(employeeId);
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
