using Application.Repositories;
using Application.Storage.Google;
using Core.CrossCuttingConcerns.Exceptions;
using Domain;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Google.Apis.Storage.v1.Data;

namespace Infrastructure.Services.Storage.Google;

public class GoogleStorage : IGoogleStorage
{
    private readonly StorageClient _storageClient;
    private readonly IOptionsSnapshot<StorageSettings> _storageSettings;
    private readonly string _bucketName = "erpaotomasyonkablo";
    
    public GoogleStorage(IConfiguration configuration, IOptionsSnapshot<StorageSettings> storageSettings)
    {
        _storageSettings = storageSettings;

        var credentialsPath = _storageSettings.Value.Providers.Google.CredentialsFilePath;
        if (string.IsNullOrEmpty(credentialsPath))
        {
            throw new BusinessException("Google Cloud Storage service account key file path is not configured.");
        }

        var credential = GoogleCredential.FromFile(credentialsPath);
        _storageClient = StorageClient.Create(credential);
    }
    
    public async Task<List<(string fileName, string path, string containerName)>> UploadFileToStorage(string entityType, string path, string fileName, MemoryStream fileStream)
    {
        List<(string fileName, string path, string containerName)> datas = new();
        string objectName = $"{entityType}/{path}/{fileName}";
        await _storageClient.UploadObjectAsync(_bucketName, objectName, null, fileStream);
        datas.Add((fileName, objectName, _bucketName));
        return datas;
    }
    
    public async Task DeleteAsync(string entityType, string path, string fileName)
    {
        try
        {
            string fullPath = $"{entityType}/{path}/{fileName}";
            await _storageClient.DeleteObjectAsync(_bucketName, fullPath);
        }
        catch 
        {
            throw new BusinessException("File not found");
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
            var fullPath = $"{entityType}/{path}/{fileName}";
            var obj = _storageClient.GetObject(_bucketName, fullPath);
            return obj != null;
        }
        catch 
        {
            return false;
        }
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