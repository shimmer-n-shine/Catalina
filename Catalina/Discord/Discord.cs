﻿using Catalina.Core;
using Catalina.Discord.Commands.TypeConverters;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Fergun.Interactive;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Reflection;
using System.Threading.Tasks;
using RunMode = Discord.Interactions.RunMode;

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
            LogLevel = LogSeverity.Debug,
            GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.All, //remove all later
            AlwaysDownloadUsers = true,
        });

        InteractionService = new InteractionService(DiscordClient, new InteractionServiceConfig
        {
            DefaultRunMode = RunMode.Async,
            LogLevel = LogSeverity.Debug
        });

        InteractiveService = new InteractiveService(DiscordClient as BaseSocketClient, new InteractiveConfig
        {
            DefaultTimeout = TimeSpan.FromSeconds(30),
            LogLevel = LogSeverity.Debug
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