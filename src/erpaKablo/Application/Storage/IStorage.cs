using Domain;
using Microsoft.AspNetCore.Http;

namespace Application.Storage;

public interface IStorage
{
    Task<List<(string fileName, string path, string category,string storageType)>> UploadAsync(string category, string path,
        List<IFormFile> files);
    Task DeleteAsync(string path);
    Task<List<T>?> GetFiles<T>(string employeeId) where T : ImageFile, new();
    bool HasFile(string path, string fileName);
}