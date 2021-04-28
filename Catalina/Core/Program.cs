using Catalina.Configuration;
using System;
using System.Threading.Tasks;
using Catalina.Discord;
using Catalina.Core;

namespace Catalina.Core
{
    class Program
    {
        static ConfigValues ConfigValues => ConfigValues.configValues;
        public static DateTime startTime;
        public static Random Random;

        static async Task Main(string[] args)
        {
            Random = new Random();
            ConfigValues.LoadConfig();
            //TrackTimeLive();

            startTime = DateTime.UtcNow;
            await Discord.Discord.SetupClient();
            await Task.Delay(-1);
        }
    }
}
