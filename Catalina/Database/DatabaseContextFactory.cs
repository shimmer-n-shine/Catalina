using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace Catalina.Database;

public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
{
    public DatabaseContext CreateDbContext(string[] args = null)
    {
        var config = new ConfigurationBuilder().AddIniFile("config.ini").Build().Get<Core.Configuration>();

        var ConnStr = config.Core.ConnectionString;
        var serverVersion = ServerVersion.AutoDetect(ConnStr);
        var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
        optionsBuilder
            .UseLazyLoadingProxies(true)
            .UseMySql(ConnStr, serverVersion, x =>
            {
                x.EnableRetryOnFailure();
            });
        return new DatabaseContext(optionsBuilder.Options);
    }
}
