using Serilog;
using System;
using System.Threading.Tasks;
using Serilog.Events;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Catalina.Database;
using Microsoft.EntityFrameworkCore;
using Serilog.Extensions.Logging;
using Microsoft.Extensions.Logging;

namespace Catalina
{
    class Program
    {
        public static ServiceProvider Services;
        static async Task Main(string[] args)
        {

            var configBuilder = new ConfigurationBuilder();
            configBuilder.AddIniFile("config.ini");
            var config = configBuilder.Build().Get<Core.Configuration>();
            IServiceCollection serviceBuilder = new ServiceCollection();

            serviceBuilder = serviceBuilder.AddSingleton<Core.Configuration>(config);
            serviceBuilder = serviceBuilder.AddDbContext<DatabaseContext>(options =>
            {
                var ConnStr = config.Core.ConnectionString;
                var serverVersion = ServerVersion.AutoDetect(ConnStr);
                options
                    .UseLazyLoadingProxies(true)
                    .UseMySql(ConnStr, serverVersion, x =>
                    {
                        x.EnableRetryOnFailure();
                    });
            }, ServiceLifetime.Transient);
            serviceBuilder = serviceBuilder.AddLogging(builder =>
            {
                var logger = new LoggerConfiguration().
                MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

                serviceBuilder = serviceBuilder.AddSingleton(logger);
                builder.AddSerilog(logger);
            });

            var services = serviceBuilder.BuildServiceProvider();
            Services = services;
            ProgramData.Random = new Random();

            ProgramData.StartTime = DateTime.UtcNow;
            await Discord.Discord.SetupClient(services);
            await Task.Delay(-1);
        }
    }

    static class ProgramData
    {
        public static DateTime StartTime;
        public static Random Random;
    }
}
