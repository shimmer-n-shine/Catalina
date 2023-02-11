using Serilog;
using System;
using System.Threading.Tasks;
using Serilog.Events;

namespace Catalina
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().
                MinimumLevel.Verbose()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            ProgramData.Random = new Random();
            AppProperties.LoadEnvVars();

            ProgramData.StartTime = DateTime.UtcNow;
            await Discord.Discord.SetupClient();
            await Task.Delay(-1);
        }
    }

    static class ProgramData
    {
        public static DateTime StartTime;
        public static Random Random;
    }
}
