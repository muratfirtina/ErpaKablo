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
            "localstorage" when HasValidUrl("LocalStorage") => 
                (IStorageProvider)_serviceProvider.GetRequiredService<ILocalStorage>(),
            "cloudinary" when HasValidUrl("Cloudinary") => 
                (IStorageProvider)_serviceProvider.GetRequiredService<ICloudinaryStorage>(),
            "google" when HasValidUrl("Google") => 
                (IStorageProvider)_serviceProvider.GetRequiredService<IGoogleStorage>(),
            _ => (IStorageProvider)_serviceProvider.GetRequiredService<ILocalStorage>() // Fallback to local
        };
    }

    public IEnumerable<IStorageProvider> GetConfiguredProviders()
    {
        var providers = new List<(string name, Func<IStorageProvider?> provider)>
        {
            ("localstorage", () => HasValidUrl("LocalStorage") ? GetProvider("localstorage") : null),
            ("cloudinary", () => HasValidUrl("Cloudinary") ? GetProvider("cloudinary") : null),
            ("google", () => HasValidUrl("Google") ? GetProvider("google") : null)
        };

        return providers
            .Select(p => p.provider())
            .Where(provider => provider != null)
            .Cast<IStorageProvider>();
    }

    private bool HasValidUrl(string providerName)
    {
        var providers = _storageSettings.Value.Providers;
        return providerName switch
        {
            "LocalStorage" => providers.LocalStorage?.Url != null,
            "Cloudinary" => providers.Cloudinary?.Url != null,
            "Google" => providers.Google?.Url != null,
            _ => false
        };
    }
}