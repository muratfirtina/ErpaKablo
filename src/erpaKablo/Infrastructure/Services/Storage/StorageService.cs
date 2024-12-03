using System.Diagnostics;
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
    private readonly IStorageProviderFactory _providerFactory;
    private readonly IOptionsSnapshot<StorageSettings> _storageSettings;
    private readonly IFileNameService _fileNameService;
    private readonly IConfiguration _configuration;


    public StorageService(
        IStorageProviderFactory providerFactory,
        IOptionsSnapshot<StorageSettings> storageSettings,
        IFileNameService fileNameService, IConfiguration configuration)
    {
        _providerFactory = providerFactory;
        _storageSettings = storageSettings;
        _fileNameService = fileNameService;
        _configuration = configuration;
    }

    public async Task<List<(string fileName, string path, string entityType, string storageType)>> UploadAsync(
        string entityType,
        string path,
        List<IFormFile> files)
    {
        var results = new List<(string fileName, string path, string entityType, string storageType)>();

        foreach (var file in files)
        {
            await _fileNameService.FileMustBeInFileFormat(file);
            string newPath = await _fileNameService.PathRenameAsync(path);

            // HasFile delegate'ini uygun şekilde oluşturuyoruz
            IFileNameService.HasFile hasFileDelegate = (pathOrContainerName, fileName) => 
                HasFile(entityType, pathOrContainerName, fileName);

            var fileNewName = await _fileNameService.FileRenameAsync(
                newPath, 
                file.FileName, 
                hasFileDelegate);

            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);

            foreach (var provider in GetConfiguredProviders())
            {
                memoryStream.Position = 0;
                var uploadResults = await provider.UploadFileToStorage(
                    entityType, 
                    newPath, 
                    fileNewName, 
                    new MemoryStream(memoryStream.ToArray())
                );

                foreach (var result in uploadResults)
                {
                    results.Add((result.fileName, result.path, entityType, provider.GetType().Name));
                }
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
        foreach (var provider in GetConfiguredProviders())
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
        //logopath i appsettings.json dan alalım.
        var logoPath = _configuration["Storage:CompanyAssets:LogoPath"];
        if (string.IsNullOrEmpty(logoPath))
            throw new Exception("Logo path not found in configuration.");

        return $"{storageUrl}/{logoPath.TrimStart('/')}";
    }

    private IEnumerable<IStorageProvider> GetConfiguredProviders()
    {
        var providers = _storageSettings.Value.Providers;
        
        if (providers.LocalStorage?.Url != null)
            yield return _providerFactory.GetProvider("localstorage");
            
        if (providers.Cloudinary?.Url != null)
            yield return _providerFactory.GetProvider("cloudinary");
            
        if (providers.Google?.Url != null)
            yield return _providerFactory.GetProvider("google");
    }
}
