namespace Application.Storage;

public interface IStorageService: IStorage
{
    public string StorageName { get; }
    string GetStorageUrl();
    Task DeleteFromAllStoragesAsync(string category, string path, string fileName);
}