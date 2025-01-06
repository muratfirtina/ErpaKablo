using Azure.Identity;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Configuration;

public static class KeyVaultConfiguration
{
    public static string GetSecretFromKeyVault(this IConfiguration configuration, string secretName, string defaultValue = "")
    {
        var value = configuration[secretName];
        return string.IsNullOrEmpty(value) ? defaultValue : value;
    }

    // Key Vault yapılandırmasını ekleyen metod
    public static WebApplicationBuilder AddKeyVaultConfiguration(this WebApplicationBuilder builder)
    {
        const string keyVaultUri = "https://erpakablo-keyvault.vault.azure.net/";
        
        try
        {
            // Azure kimlik bilgileri için özel ayarlar
            var credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions
            {
                // Development ortamında CLI ve Environment kimlik doğrulamasını kullan
                ExcludeEnvironmentCredential = false,
                ExcludeAzureCliCredential = false,
                // Visual Studio kimlik doğrulamasını devre dışı bırak
                ExcludeVisualStudioCredential = true,
                ExcludeVisualStudioCodeCredential = true,
                
                // Detaylı loglama ayarları
                Diagnostics =
                {
                    LoggedHeaderNames = { "x-ms-request-id" },
                    LoggedQueryParameters = { "api-version" },
                    IsLoggingEnabled = true
                }
            });

            // Key Vault'u yapılandırmaya ekle
            builder.Configuration.AddAzureKeyVault(
                new Uri(keyVaultUri), 
                credential);

            // Bağlantıyı test et
            ValidateKeyVaultConnection(builder.Configuration);

            return builder;
        }
        catch (Exception ex)
        {
            HandleKeyVaultConfigurationError(ex, keyVaultUri);
            throw;
        }
    }

    // Key Vault bağlantısını doğrula
    private static void ValidateKeyVaultConnection(IConfiguration configuration)
    {
        var testSecret = configuration["JwtSecurityKey"];
        if (string.IsNullOrEmpty(testSecret))
        {
            throw new InvalidOperationException(
                """
                Key Vault bağlantısı başarılı ancak gizli değerlere erişilemiyor.
                Lütfen şunları kontrol edin:
                1. RBAC rolleri doğru atanmış mı?
                2. Gizli değerler Key Vault'a yüklenmiş mi?
                3. Gizli değerlerin isimleri doğru mu?
                """);
        }
    }

    // Hata durumunu yönet
    private static void HandleKeyVaultConfigurationError(Exception ex, string keyVaultUri)
    {
        var errorMessage = $"""
            Key Vault yapılandırması başarısız oldu!
            
            Olası nedenler ve çözümler:
            1. Azure CLI ile giriş yapılmamış olabilir
               Çözüm: Terminal'de 'az login' komutunu çalıştırın
               
            2. Key Vault URI yanlış olabilir
               Kontrol edilecek URI: {keyVaultUri}
               
            3. RBAC rolleri eksik olabilir
               Gerekli rol: Key Vault Secrets User
               Çözüm: Azure Portal'dan rol atamasını kontrol edin
               
            4. Ağ erişimi engellenmiş olabilir
               Key Vault güvenlik duvarı ayarlarını kontrol edin
            
            Teknik Detaylar:
            {ex.Message}
            
            Stack Trace:
            {ex.StackTrace}
            """;
        
        throw new InvalidOperationException(errorMessage, ex);
    }
}