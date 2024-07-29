using Application.Repositories;
using Application.Storage.Google;
using Core.CrossCuttingConcerns.Exceptions;
using Domain;
using Google.Apis.Auth.OAuth2;
using Google.Cloud.Storage.V1;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;

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
        await _storageClient.UploadObjectAsync("erpaotomasyonkablo", $"{category}/{path}/{fileName}", null, fileStream);
        
        return null;
        
        
    }
    
    public async Task DeleteAsync(string path)
    {
        var storage = StorageClient.Create();
        await storage.DeleteObjectAsync("erpaotomasyonkablo", path);
    }
    
    public async Task<List<T>?> GetFiles<T>(string productId) where T : ImageFile, new()
    {
        var baseUrl = _storageSettings.GoogleStorageUrl; // Ayarlardan URL alınır
        //employeeId ye göre category ve path bul.
        var productImages = await _productRepository.GetFilesByProductId(productId);
        if (productImages == null || !productImages.Any())
            return null; 

        List<T> files = new List<T>();

        foreach (var productImageDto in productImages) 
        {
            var prefix = $"{productImageDto.Category}/{productImageDto.Path}/";
            var objects = _storageClient.ListObjects(_bucketName, prefix);

            // Assuming your _storageClient handles finding the relevant file based on the metadata
            var matchingObject = objects.FirstOrDefault(obj => obj.Name.EndsWith(productImageDto.FileName)); 

            if (matchingObject != null)
            {
                var file = new T
                {
                    Id = productImageDto.Id,
                    Name = productImageDto.FileName,
                    Path = productImageDto.Path,
                    Category = productImageDto.Category,
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
            // Google Cloud Storage'da belirtilen bucket ve dosya adıyla bir obje olup olmadığını kontrol edin
            var obj = _storageClient.GetObject("erpaotomasyonkablo", $"{path}/{fileName}");
            return obj != null; // Eğer obje null değilse, dosya var demektir
        }
        
        catch (Exception ex)
        {
            // Diğer hatalar için bir loglama yapısı ekleyebilirsiniz
            // Bu örnek için basitçe false dönüyoruz, ancak gerçek bir uygulamada hata yönetimi daha detaylı olmalıdır
            Console.WriteLine($"An error occurred: {ex.Message}");
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