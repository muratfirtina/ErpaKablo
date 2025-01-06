using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Text;

namespace Persistence.DbConfiguration;

/// <summary>
/// Veritabanı bağlantı bilgilerini Azure Key Vault'tan güvenli bir şekilde almaktan sorumlu sınıf.
/// </summary>
public static class DatabaseConfiguration
{
    public static string GetConnectionString(IConfiguration configuration, ILogger logger)
    {
        try
        {
            return GetConnectionStringFromKeyVault(configuration, logger);
        }
        catch (Exception ex)
        {
            var error = new StringBuilder()
                .AppendLine("Veritabanı bağlantı bilgileri Key Vault'tan alınamadı.")
                .AppendLine("Lütfen aşağıdaki kontrolleri yapın:")
                .AppendLine("1. Azure Key Vault erişim bilgileri doğru mu?")
                .AppendLine("2. Gerekli tüm secret'lar Key Vault'a eklenmiş mi?")
                .AppendLine("3. Azure CLI ile giriş yapılmış mı? (az login)")
                .AppendLine($"Hata Detayı: {ex.Message}")
                .ToString();

            logger.LogError(ex, error);
            throw new InvalidOperationException(error, ex);
        }
    }

    private static string GetConnectionStringFromKeyVault(IConfiguration configuration, ILogger logger)
    {
        // Key Vault URI'sini al ve kontrol et
        var keyVaultUri = configuration["AzureKeyVault:VaultUri"] 
            ?? throw new InvalidOperationException("Key Vault URI bulunamadı.");

        // URI'nin geçerli olduğundan emin ol
        if (!Uri.TryCreate(keyVaultUri, UriKind.Absolute, out var uri))
        {
            logger.LogError("Geçersiz Key Vault URI: {Uri}", keyVaultUri);
            throw new InvalidOperationException($"Geçersiz Key Vault URI formatı: {keyVaultUri}");
        }
        
        logger.LogInformation("Key Vault'a bağlanılıyor: {Uri}", uri);

        // Key Vault credentials
        var credential = new ClientSecretCredential(
            configuration["AzureKeyVault:TenantId"],
            configuration["AzureKeyVault:ClientId"],
            configuration["AzureKeyVault:ClientSecret"]
        );

        // Key Vault'a bağlan ve configuration oluştur
        var keyVaultConfiguration = new ConfigurationBuilder()
            .AddAzureKeyVault(uri, credential)
            .Build();

        // Veritabanı secretlarını al
        var secrets = new Dictionary<string, string>
        {
            { "PostgresUser", keyVaultConfiguration["PostgresUser"] },
            { "PostgresPassword", keyVaultConfiguration["PostgresPassword"] },
            { "PostgresHost", keyVaultConfiguration["PostgresHost"] },
            { "PostgresPort", keyVaultConfiguration["PostgresPort"] },
            { "PostgresDatabase", keyVaultConfiguration["PostgresDatabase"] }
        };
        

        // Eksik secret kontrolü
        var missingSecrets = secrets.Where(s => string.IsNullOrEmpty(s.Value))
                                  .Select(s => s.Key)
                                  .ToList();

        if (missingSecrets.Any())
        {
            var errorMessage = $"Aşağıdaki secret'lar Key Vault'ta bulunamadı: {string.Join(", ", missingSecrets)}";
            logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        logger.LogInformation("Veritabanı bağlantı bilgileri başarıyla alındı");

        // Connection string oluştur
        return $"User ID={secrets["PostgresUser"]};" +
               $"Password={secrets["PostgresPassword"]};" +
               $"Host={secrets["PostgresHost"]};" +
               $"Port={secrets["PostgresPort"]};" +
               $"Database={secrets["PostgresDatabase"]};" +
               "Enlist=true;" +      // TransactionScope desteği
               "Pooling=true;";      // Connection pooling
    }
}