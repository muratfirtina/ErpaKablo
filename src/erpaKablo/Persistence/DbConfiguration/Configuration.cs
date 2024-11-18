using Microsoft.Extensions.Configuration;

namespace Persistence.DbConfiguration;

static class Configuration
{
    static public string ConnectionString
    {
        get
        {
            ConfigurationManager configurationManager = new();
            configurationManager.SetBasePath(Path.Combine(Directory.GetCurrentDirectory(), "../WebAPI"));
            configurationManager.AddJsonFile("appsettings.json");
            return configurationManager.GetConnectionString("ErpaKabloDb");
            
        }
        
    }
}