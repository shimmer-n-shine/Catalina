using System;
using System.Threading.Tasks;
using Discord;
using Fergun.Interactive;
using Discord.WebSocket;
using System.Reflection;
using Discord.Interactions;
using RunMode = Discord.Interactions.RunMode;

namespace Catalina.Discord
{
    public class Discord 
    {
        public static readonly DiscordSocketClient DiscordClient;
        public static readonly InteractiveService InteractiveService;
        public static readonly InteractionService InteractionService;
        static Discord()
        {
            DiscordClient = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.All, //remove all later
                AlwaysDownloadUsers = true,
            });

            InteractionService = new InteractionService(DiscordClient, new InteractionServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Verbose
            });

            InteractiveService = new InteractiveService(DiscordClient as BaseSocketClient, new InteractiveConfig
            {
                DefaultTimeout = TimeSpan.FromSeconds(30),
                LogLevel = LogSeverity.Verbose
            });
        }
        public static async Task SetupClient()
        {         
            await InteractionService.AddModulesAsync(Assembly.GetExecutingAssembly(), null);


            DiscordClient.GuildMemberUpdated += Events.GuildMemberUpdated;
            DiscordClient.Ready += Events.Ready;
            DiscordClient.Log += Events.DiscordLog;

            await DiscordClient.LoginAsync(TokenType.Bot, AppConfig.DiscordToken);

            await DiscordClient.StartAsync();
        }
    }
}