using System;
using System.Threading.Tasks;
using Discord;
using Fergun.Interactive;
using Discord.WebSocket;
using System.Reflection;
using Discord.Interactions;
using RunMode = Discord.Interactions.RunMode;
using Catalina.Discord.Commands.TypeConverters;
using Catalina.Core;
using Microsoft.Extensions.DependencyInjection;

namespace Catalina.Discord;

public class Discord
{
    public static readonly DiscordSocketClient DiscordClient;
    public static readonly InteractiveService InteractiveService;
    public static readonly InteractionService InteractionService;
    private static ServiceProvider _services;
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
    public static async Task SetupClient(ServiceProvider services)
    {
        _services = services;
        Events.Events.Services = services;
        Starboard.Services = services;
        InteractionService.AddTypeConverter<Color>(new ColorTypeConverter());

        await InteractionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
        

        DiscordClient.UserJoined += Events.Events.GuildMemberAdded;
        DiscordClient.InteractionCreated += Events.Events.InteractionCreated;
        DiscordClient.ReactionAdded += Events.Events.ReactionAdded;
        DiscordClient.Ready += Events.Events.Ready;


        DiscordClient.Log += Events.Events.DiscordLog;

        await DiscordClient.LoginAsync(TokenType.Bot, _services.GetRequiredService<Configuration>().Core.DiscordToken);

        await DiscordClient.StartAsync();
    }
}