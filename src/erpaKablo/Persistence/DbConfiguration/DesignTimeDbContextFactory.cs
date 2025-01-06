using dotenv.net;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Persistence.Context;

namespace Persistence.DbConfiguration;

/// <summary>
/// Entity Framework Core migrations için design-time DbContext factory.
/// Migration komutları çalıştırıldığında bu factory kullanılır.
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ErpaKabloDbContext>
{
    public ErpaKabloDbContext CreateDbContext(string[] args)
    {
        var loggerFactory = LoggerFactory.Create(builder => 
            builder.AddConsole()
                .SetMinimumLevel(LogLevel.Information));
    
        var logger = loggerFactory.CreateLogger<DesignTimeDbContextFactory>();

        try
        {
            // Dizin yapısını belirle
            var currentDirectory = Directory.GetCurrentDirectory();
            logger.LogInformation("Başlangıç konumu (Persistence): {Directory}", currentDirectory);

            var erpakabloDirectory = Directory.GetParent(currentDirectory)?.FullName 
                ?? throw new InvalidOperationException("ErpaKablo dizini bulunamadı.");
            logger.LogInformation("ErpaKablo dizini: {Directory}", erpakabloDirectory);

            var srcDirectory = Directory.GetParent(erpakabloDirectory)?.FullName 
                ?? throw new InvalidOperationException("Src dizini bulunamadı.");
            logger.LogInformation("Src dizini (.env konumu): {Directory}", srcDirectory);

            var webApiDirectory = Path.Combine(erpakabloDirectory, "WebAPI");
            logger.LogInformation("WebAPI dizini (appsettings.json konumu): {Directory}", webApiDirectory);

            // .env dosyasını kontrol et ve yükle
            var envPath = Path.Combine(srcDirectory, ".env");
            if (!File.Exists(envPath))
            {
                throw new FileNotFoundException($".env dosyası bulunamadı. Beklenen konum: {envPath}");
            }
            logger.LogInformation(".env dosyası bulundu: {Path}", envPath);

            DotEnv.Load(new DotEnvOptions(
                envFilePaths: new[] { envPath },
                trimValues: true
            ));

            // Environment variables'ları kontrol et
            var keyVaultUri = Environment.GetEnvironmentVariable("AZURE_KEYVAULT_URI") 
                ?? throw new InvalidOperationException("AZURE_KEYVAULT_URI environment variable not found");
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID")
                ?? throw new InvalidOperationException("AZURE_TENANT_ID environment variable not found");
            var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID")
                ?? throw new InvalidOperationException("AZURE_CLIENT_ID environment variable not found");
            var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET")
                ?? throw new InvalidOperationException("AZURE_CLIENT_SECRET environment variable not found");

            // Configuration oluştur
            var configuration = new ConfigurationBuilder()
                .SetBasePath(webApiDirectory)
                .AddJsonFile("appsettings.json", optional: false)
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    ["AzureKeyVault:VaultUri"] = keyVaultUri,
                    ["AzureKeyVault:TenantId"] = tenantId,
                    ["AzureKeyVault:ClientId"] = clientId,
                    ["AzureKeyVault:ClientSecret"] = clientSecret
                })
                .Build();

            logger.LogInformation("Yapılandırma sistemi başarıyla oluşturuldu");

            // DatabaseConfiguration üzerinden connection string'i al
            var connectionString = DatabaseConfiguration.GetConnectionString(configuration, logger);
            
            // DbContext options'ları oluştur
            var optionsBuilder = new DbContextOptionsBuilder<ErpaKabloDbContext>();
            optionsBuilder
                .UseNpgsql(connectionString)
                .EnableSensitiveDataLogging()  // Development ortamında SQL loglarını görmek için
                .LogTo(
                    message => logger.LogInformation(message), 
                    LogLevel.Information
                );

            return new ErpaKabloDbContext(optionsBuilder.Options);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "DbContext oluşturulurken hata meydana geldi");
            throw;
        }
    }
}