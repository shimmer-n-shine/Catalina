using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Proxies;
using System;

namespace Catalina.Database
{
    public class DatabaseContextFactory : IDesignTimeDbContextFactory<DatabaseContext>
    {
        public DatabaseContext CreateDbContext(string[] args = null)
        {
            AppProperties.LoadEnvVars();
            var ConnStr = Environment.GetEnvironmentVariable(AppProperties.ConnectionString);
            var serverVersion = ServerVersion.AutoDetect(ConnStr);
            var optionsBuilder = new DbContextOptionsBuilder<DatabaseContext>();
            optionsBuilder
                .UseMySql(ConnStr, serverVersion, x =>
                {
                    x.EnableRetryOnFailure();
                })
                .UseLazyLoadingProxies(true);
            return new DatabaseContext(optionsBuilder.Options);
        }
    }
}
