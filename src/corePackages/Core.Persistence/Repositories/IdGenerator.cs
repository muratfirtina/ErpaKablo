using Core.Persistence.Repositories.Operation;

namespace Core.Persistence.Repositories;

public static class IdGenerator
{
    public static string GenerateId(string name)
    {
        string cleanedName = NameOperation.CharacterRegulatory(name.ToLower());

        // Ardından rastgele bir alfanumerik string (GUID'dan türetilmiş) ekleyerek son ID'yi oluştur
        var randomPart = Guid.NewGuid().ToString("N").Substring(0, 16); // 16 karakterlik rastgele bir dizi

        // Düzenlenmiş ad ve rastgele kısmı birleştir
        return $"{cleanedName}-{randomPart}";
    }
}