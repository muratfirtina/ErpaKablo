namespace Application.Abstraction.Services.Configurations;

public interface IKeyVaultService
{
    /// <summary>
    /// Azure Key Vault'tan belirtilen isimli secret'ı getirir
    /// </summary>
    /// <param name="secretName">Secret'ın adı</param>
    /// <returns>Secret değeri</returns>
    /// <exception cref="SecurityException">Secret alınamazsa fırlatılır</exception>
    Task<string> GetSecretAsync(string secretName);

    /// <summary>
    /// Azure Key Vault'a yeni bir secret ekler veya var olanı günceller
    /// </summary>
    /// <param name="secretName">Secret'ın adı</param>
    /// <param name="value">Secret'ın değeri</param>
    /// <exception cref="SecurityException">Secret kaydedilemezse fırlatılır</exception>
    Task SetSecretAsync(string secretName, string value);

    /// <summary>
    /// Azure Key Vault'taki tüm secret'ları getirir
    /// </summary>
    /// <returns>Secret adı ve değerlerini içeren dictionary</returns>
    /// <exception cref="SecurityException">Secret'lar alınamazsa fırlatılır</exception>
    Task<Dictionary<string, string>> GetAllSecretsAsync();
}