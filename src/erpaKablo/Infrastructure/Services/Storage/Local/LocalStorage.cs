using Application.Repositories;
using Application.Storage.Local;
using Core.CrossCuttingConcerns.Exceptions;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Storage.Local;

public class LocalStorage : ILocalStorage
{
    private readonly IImageFileRepository _imageFileRepository;
    private readonly string _baseFolderPath = Path.Combine("wwwroot");
    private readonly IOptionsSnapshot<StorageSettings> _storageSettings;
    
    public LocalStorage(IOptionsSnapshot<StorageSettings> storageSettings, IImageFileRepository imageFileRepository)
    {
        
        _imageFileRepository = imageFileRepository;
        _storageSettings = storageSettings;
        if (!Directory.Exists(_baseFolderPath))
        {
            Directory.CreateDirectory(_baseFolderPath);
        }
    }

    public async Task<List<(string fileName, string path, string containerName)>> UploadFileToStorage(string category,
        string path, string fileName, MemoryStream fileStream)
    {
        var employeeFolderPath = Path.Combine(_baseFolderPath, category, path);
        
        if (!Directory.Exists(employeeFolderPath))
        {
            Directory.CreateDirectory(employeeFolderPath);
        }
        
        List<(string fileName, string path, string containerName)> datas = new();
        
        //dosyayı locale kaydet
        var filePath = Path.Combine(employeeFolderPath, fileName);
        await using FileStream fileStream1 = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024, useAsync:false);
        await fileStream.CopyToAsync(fileStream1);
        await fileStream1.FlushAsync();
        
        datas.Add((fileName, path, category));

        return null;
    }

    public async Task DeleteAsync(string path)
    {
        //var localPath = ExtractLocalPath(path); // Dosya yolu çıkar
        var filePath = Path.Combine(_baseFolderPath, path); // Dosya yolu ve adını birleştir
        if (File.Exists(filePath))
        {
            File.Delete(filePath);
        }
        
    }
    

    public async Task<List<T>> GetFiles<T>(string productId) where T : ImageFile, new()
    {
        var baseUrl = _storageSettings.Value.Providers.LocalStorage.Url;
        var productFolder = Path.Combine(_baseFolderPath, "products", productId);
        
        if (!Directory.Exists(productFolder))
        {
            return new List<T>();
        }

        var files = Directory.GetFiles(productFolder, "*", SearchOption.AllDirectories);
        var result = new List<T>();

        foreach (var file in files)
        {
            var relativePath = Path.GetRelativePath(_baseFolderPath, file);
            var fileInfo = new FileInfo(file);
            var category = Path.GetDirectoryName(relativePath)?.Split(Path.DirectorySeparatorChar)[0] ?? "unknown";

            result.Add(new T
            {
                Id = Path.GetFileNameWithoutExtension(file),
                Name = fileInfo.Name,
                Path = relativePath,
                Category = category,
                Storage = "LocalStorage",
                Url = $"{baseUrl.TrimEnd('/')}/{relativePath.Replace('\\', '/')}"
            });
        }

        return result;
    }

    public bool HasFile(string path, string fileName) 
        => File.Exists(Path.Combine(path, fileName));
    
    async Task<bool> CopyFileAsync(string path, IFormFile file)
    {
        try
        {
            await using FileStream fileStream = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None, 1024 * 1024, useAsync:false);
            
            await file.CopyToAsync(fileStream);
            await fileStream.FlushAsync();
            return true;

        }
        catch (Exception e)
        {
            //todo: loglama yapılacak!
            throw e;
        }

    }
    
    public async Task FileMustBeInImageFormat(IFormFile formFile)
    {
        List<string> extensions = new() { ".jpg", ".png", ".jpeg", ".webp", ".heic" };

        string extension = Path.GetExtension(formFile.FileName).ToLower();
        if (!extensions.Contains(extension))
            throw new BusinessException("Unsupported format");
        await Task.CompletedTask;
    }
    
    public async Task FileMustBeInFileFormat(IFormFile formFile)
    {
        List<string> extensions = new() { ".jpg", ".png", ".jpeg", ".webp", ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".heic" };

        string extension = Path.GetExtension(formFile.FileName).ToLower();
        if (!extensions.Contains(extension))
            throw new BusinessException("Unsupported format");
        await Task.CompletedTask;
    }
}