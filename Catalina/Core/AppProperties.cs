using dotenv.net;
using System;
using System.IO;

namespace Catalina
{
    public static class AppProperties
    {
        public const string ConnectionString = "DBCONNECTIONSTR";
        public const string BotPrefix = "BOTPREFIX";
        public const string DiscordToken = "DISCTOKEN";
        public const string DeveloperID = "DEVID";
        public const string DeveloperGuildID = "DEVGUILDID";


        public static void LoadEnvVars()
        {
            DotEnv.Load(new DotEnvOptions(envFilePaths: new[] { Path.Combine(AppContext.BaseDirectory, ".env") }));
        }
    }
}
