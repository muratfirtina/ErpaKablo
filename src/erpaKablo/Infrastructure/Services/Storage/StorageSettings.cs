namespace Infrastructure.Services.Storage;

public class StorageSettings
{
    public string LocalStorageUrl { get; set; }
    public string AzureStorageUrl { get; set; }
    public string GoogleStorageUrl { get; set; }
    public string AWSStorageUrl { get; set; }
}