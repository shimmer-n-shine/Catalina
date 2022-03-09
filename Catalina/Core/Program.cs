using System;
using System.Threading.Tasks;

namespace Catalina
{
    class Program
    {
        public static DateTime startTime;
        public static Random Random;

        static async Task Main(string[] args)
        {
            Random = new Random();
            AppProperties.LoadEnvVars();

            startTime = DateTime.UtcNow;
            await Discord.Discord.SetupClient();
            await Task.Delay(-1);
        }
    }
}
