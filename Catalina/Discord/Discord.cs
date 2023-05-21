﻿using System;
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

namespace Catalina.Discord
{
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
            Events.Services = services;
            Starboard.Services = services;
            InteractionService.AddTypeConverter<Color>(new ColorTypeConverter());

            await InteractionService.AddModulesAsync(Assembly.GetExecutingAssembly(), _services);
            

            DiscordClient.UserJoined += Events.GuildMemberAdded;
            DiscordClient.MessageDeleted += Events.MessageDeleted;
            DiscordClient.MessageReceived += Events.MessageCreated;
            DiscordClient.InteractionCreated += Events.InteractionCreated;
            DiscordClient.ReactionAdded += Events.ReactionAdded;
            DiscordClient.ReactionRemoved += Events.ReactionRemoved;
            DiscordClient.ReactionsCleared += Events.ReactionsCleared;
            DiscordClient.JoinedGuild += Events.JoinedGuild;
            DiscordClient.LeftGuild += Events.LeftGuild;
            DiscordClient.Ready += Events.Ready;


            DiscordClient.Log += Events.DiscordLog;

            await DiscordClient.LoginAsync(TokenType.Bot, _services.GetRequiredService<Configuration>().Core.DiscordToken);

            await DiscordClient.StartAsync();
        }
    }
}