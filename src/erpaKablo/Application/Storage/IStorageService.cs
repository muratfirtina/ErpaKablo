namespace Application.Storage;

public interface IStorageService: IStorage
{
    public string StorageName { get; }
}