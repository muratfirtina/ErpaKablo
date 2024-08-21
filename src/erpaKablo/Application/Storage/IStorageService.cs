using Domain;
using Microsoft.AspNetCore.Http;

namespace Application.Storage;

public interface IStorageService
{
    Task<List<(string fileName, string path, string entityType, string storageType)>> UploadAsync(string entityType, string path, List<IFormFile> files);
    Task DeleteFromAllStoragesAsync(string entityType, string path, string fileName);
    Task<List<T>?> GetFiles<T>(string entityId, string entityType, string preferredStorage = null) where T : ImageFile, new();
    bool HasFile(string entityType, string path, string fileName);
    string GetStorageUrl(string storageType = null);
}