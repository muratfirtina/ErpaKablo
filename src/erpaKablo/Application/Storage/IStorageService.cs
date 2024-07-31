using Domain;
using Microsoft.AspNetCore.Http;

namespace Application.Storage;

public interface IStorageService
{
    Task<List<(string fileName, string path, string category, string storageType)>> UploadAsync(string category, string path, List<IFormFile> files);
    Task DeleteFromAllStoragesAsync(string category, string path, string fileName);
    Task<List<T>?> GetFiles<T>(string productId, string preferredStorage = null) where T : ImageFile, new();
    bool HasFile(string path, string fileName);
    string GetStorageUrl(string storageType = null);
}