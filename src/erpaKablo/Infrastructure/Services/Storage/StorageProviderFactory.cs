using Application.Storage;
using Application.Storage.Cloudinary;
using Application.Storage.Google;
using Application.Storage.Local;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Infrastructure.Services.Storage;

public class StorageProviderFactory : IStorageProviderFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IOptionsSnapshot<StorageSettings> _storageSettings;

    public StorageProviderFactory(
        IServiceProvider serviceProvider,
        IOptionsSnapshot<StorageSettings> storageSettings)
    {
        _serviceProvider = serviceProvider;
        _storageSettings = storageSettings;
    }

    public IStorageProvider GetProvider(string? providerName = null)
    {
        var activeProvider = providerName ?? _storageSettings.Value.ActiveProvider ?? "localstorage";
        
        return activeProvider.ToLower() switch
        {
            "localstorage" => (IStorageProvider)_serviceProvider.GetRequiredService<ILocalStorage>(),
            "cloudinary" => (IStorageProvider)_serviceProvider.GetRequiredService<ICloudinaryStorage>(),
            "google" => (IStorageProvider)_serviceProvider.GetRequiredService<IGoogleStorage>(),
            _ => throw new ArgumentException($"Unsupported storage provider: {providerName}")
        };
    }
}