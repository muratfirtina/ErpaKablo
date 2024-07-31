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
    private readonly IProductRepository _productRepository;
    private readonly StorageSettings _storageSettings;
    private readonly string _bucketName = "erpaotomasyonkablo";
    
    public GoogleStorage(IConfiguration configuration, IProductRepository productRepository, IOptions<StorageSettings> storageSettings)
    {
        _productRepository = productRepository;
        _storageSettings = storageSettings.Value;

        var credentialsPath = configuration["Storage:Google:CredentialsFilePath"];
        if (string.IsNullOrEmpty(credentialsPath))
        {
            throw new BusinessException("Google Cloud Storage service account key file path is not configured.");
        }

        var credential = GoogleCredential.FromFile(credentialsPath);
        _storageClient = StorageClient.Create(credential);
    }
    
    public async Task<List<(string fileName, string path, string containerName)>> UploadFileToStorage(string category,
        string path, string fileName, MemoryStream fileStream)
    {
        List<(string fileName, string path, string containerName)> datas = new();
        string objectName = $"products/{path}/{fileName}";
        await _storageClient.UploadObjectAsync(_bucketName, objectName, null, fileStream);
        datas.Add((fileName, objectName, _bucketName));
        return datas;
    }
    
    public async Task DeleteAsync(string fullPath)
    {
        try
        {
            await _storageClient.DeleteObjectAsync(_bucketName, fullPath);
        }
        catch 
        {
            throw new BusinessException("File not found");
        }
    }
    
    public async Task<List<T>?> GetFiles<T>(string productId) where T : ImageFile, new()
    {
        var baseUrl = _storageSettings.GoogleStorageUrl;
        var productImages = await _productRepository.GetFilesByProductId(productId);
        if (productImages == null || !productImages.Any())
            return null; 

        List<T> files = new List<T>();

        foreach (var productImageDto in productImages) 
        {
            var prefix = $"products/{productImageDto.Path}/";
            var objects = _storageClient.ListObjects(_bucketName, prefix);

            var matchingObject = objects.FirstOrDefault(obj => obj.Name.EndsWith(productImageDto.FileName)); 

            if (matchingObject != null)
            {
                var file = new T
                {
                    Id = productImageDto.Id,
                    Name = productImageDto.FileName,
                    Path = productImageDto.Path,
                    Category = "products",
                    Storage = productImageDto.Storage, 
                    Url = $"{baseUrl}/{matchingObject.Name}"
                };
                files.Add(file);
            }
        }

        return files;
    }

    public bool HasFile(string path, string fileName)
    {
        try
        {
            var fullPath = $"products/{path}/{fileName}";
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