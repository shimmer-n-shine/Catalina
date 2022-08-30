using System;
using System.Threading.Tasks;

namespace Catalina
{
    class Program
    {
        static async Task Main(string[] args)
        {
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
