using System.Diagnostics;
using Application.Services;
using Application.Storage;
using Application.Storage.Local;
using Domain;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Services.Storage;

public class StorageService : IStorageService
{
    private readonly IStorageProviderFactory _providerFactory;
    private readonly IFileNameService _fileNameService;
    private readonly IConfiguration _configuration;

    public StorageService(
        IStorageProviderFactory providerFactory,
        IFileNameService fileNameService,
        IConfiguration configuration)
    {
        _providerFactory = providerFactory;
        _fileNameService = fileNameService;
        _configuration = configuration;
    }

    public async Task<List<(string fileName, string path, string entityType, string storageType, string url, string format)>> UploadAsync(
        string entityType,
        string path,
        List<IFormFile> files)
    {
        var results = new List<(string fileName, string path, string entityType, string storageType, string url, string format)>();
        
        foreach (var file in files)
        {
            var (newPath, fileNewName) = await PrepareFileDetails(file, entityType, path);
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            var uploadTasks = _providerFactory.GetConfiguredProviders()
                .Select(provider => UploadToProvider(provider, entityType, newPath, fileNewName, memoryStream))
                .ToList();

            try
            {
                await Task.WhenAll(uploadTasks);

                var localStorageTask = uploadTasks
                    .Select(t => t.Result)
                    .FirstOrDefault(t => t?.Provider is ILocalStorage);

                if (localStorageTask?.Result != null)
                {
                    foreach (var result in localStorageTask.Result)
                    {
                        var baseUrl = GetStorageUrl()?.TrimEnd('/');
                        var format = Path.GetExtension(file.FileName).TrimStart('.').ToLower();
                        var url = $"{baseUrl}/{entityType}/{result.path}/{result.fileName}";
                        results.Add((result.fileName, result.path, entityType, "localstorage", url,format));
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error during file upload: {ex.Message}");
            }
        }

        return results;
    }

    public async Task<List<T>?> GetFiles<T>(
        string entityId,
        string entityType,
        string? preferredStorage = null) where T : ImageFile, new()
    {
        var provider = _providerFactory.GetProvider(preferredStorage);
        return await provider.GetFiles<T>(entityId, entityType);
    }

    public bool HasFile(string entityType, string path, string fileName)
    {
        var provider = _providerFactory.GetProvider(null);
        return provider.HasFile(entityType, path, fileName);
    }

    public string GetStorageUrl(string? storageType = null)
    {
        var provider = _providerFactory.GetProvider(storageType);
        return provider.GetStorageUrl();
    }

    public async Task DeleteFromAllStoragesAsync(string entityType, string path, string fileName)
    {
        var providers = _providerFactory.GetConfiguredProviders();
        foreach (var provider in providers)
        {
            try
            {
                await provider.DeleteAsync(entityType, path, fileName);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting from {provider.GetType().Name}: {ex.Message}");
            }
        }
    }

    public string GetCompanyLogoUrl()
    {
        var storageUrl = GetStorageUrl()?.TrimEnd('/');
        var logoPath = _configuration["Storage:CompanyAssets:LogoPath"];
        if (string.IsNullOrEmpty(logoPath))
            throw new Exception("Logo path not found in configuration.");

        return $"{storageUrl}/{logoPath.TrimStart('/')}";
    }

    private async Task<(string newPath, string fileName)> PrepareFileDetails(
        IFormFile file,
        string entityType,
        string path)
    {
        await _fileNameService.FileMustBeInFileFormat(file);
        string newPath = await _fileNameService.PathRenameAsync(path);

        IFileNameService.HasFile hasFileDelegate = (pathOrContainerName, fileName) =>
            HasFile(entityType, pathOrContainerName, fileName);

        var fileNewName = await _fileNameService.FileRenameAsync(
            newPath,
            file.FileName,
            hasFileDelegate);

        return (newPath, fileNewName);
    }

    private class UploadTask
    {
        public IStorageProvider? Provider { get; init; }
        public List<(string fileName, string path, string containerName)>? Result { get; set; }
    }

    private async Task<UploadTask> UploadToProvider(
        IStorageProvider provider,
        string entityType,
        string path,
        string fileName,
        MemoryStream memoryStream)
    {
        var uploadTask = new UploadTask { Provider = provider };

        try
        {
            memoryStream.Position = 0;
            var result = await provider.UploadFileToStorage(
                entityType,
                path,
                fileName,
                new MemoryStream(memoryStream.ToArray())
            );
            uploadTask.Result = result;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error uploading to {provider.GetType().Name}: {ex.Message}");
        }

        return uploadTask;
    }
}