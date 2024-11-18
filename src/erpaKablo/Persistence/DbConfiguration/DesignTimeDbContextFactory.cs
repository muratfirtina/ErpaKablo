using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Persistence.Context;

namespace Persistence.DbConfiguration;

public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ErpaKabloDbContext>
{

    public ErpaKabloDbContext CreateDbContext(string[] args)
    {
        var dbContextOptionsBuilder = new DbContextOptionsBuilder<ErpaKabloDbContext>();
        dbContextOptionsBuilder.UseNpgsql(Configuration.ConnectionString).EnableSensitiveDataLogging();
        
        
        return new ErpaKabloDbContext(dbContextOptionsBuilder.Options);
    }
    
}