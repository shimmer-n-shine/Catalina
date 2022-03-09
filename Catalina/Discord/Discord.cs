using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Catalina.Database;
using Microsoft.EntityFrameworkCore;
using Discord;
using Fergun.Interactive;
using Discord.WebSocket;
using Discord.Commands;
using System.Reflection;
using NLog;
using System.Threading;
using Discord.Interactions;
using RunMode = Discord.Interactions.RunMode;
using Catalina.Discord.Commands.TypeConverters;

namespace Catalina.Discord
{
    public class Discord
    {
        public static DiscordSocketClient discord;
        public static InteractiveService interactiveService;
        public static InteractionService interactionService;
        public static async Task SetupClient()
        {
            var logger = LogManager.GetCurrentClassLogger();

            discord = new DiscordSocketClient(new DiscordSocketConfig()
            {
                LogLevel = LogSeverity.Verbose,
                GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.All, //remove all later
                //Token = Environment.GetEnvironmentVariable(AppProperties.DiscordToken),
                //TokenType = TokenType.Bot,
                AlwaysDownloadUsers = true,
            });

            interactionService = new InteractionService(discord, new InteractionServiceConfig
            {
                DefaultRunMode = RunMode.Async,
                LogLevel = LogSeverity.Verbose
            });

            interactiveService = new InteractiveService(discord as BaseSocketClient, new InteractiveConfig
            {
                DefaultTimeout = TimeSpan.FromSeconds(30),
                LogLevel = LogSeverity.Verbose
            });

            interactionService.AddTypeConverter<bool?>(new NullableBoolTypeConverter());
            interactionService.AddTypeConverter<Color>(new ColorTypeConverter());

            await interactionService.AddModulesAsync(Assembly.GetExecutingAssembly(), null);
            

            discord.UserJoined += Events.GuildMemberAdded;
            discord.MessageDeleted += Events.MessageDeleted;
            discord.MessageReceived += Events.MessageCreated;
            discord.InteractionCreated += Events.InteractionCreated;
            discord.ReactionAdded += Events.ReactionAdded;
            discord.ReactionRemoved += Events.ReactionRemoved;
            discord.ReactionsCleared += Events.ReactionsCleared;
            discord.JoinedGuild += Events.JoinedGuild;
            discord.LeftGuild += Events.LeftGuild;
            discord.Ready += Events.Ready;

            discord.Log += Events.Discord_Log;

            await discord.LoginAsync(TokenType.Bot, Environment.GetEnvironmentVariable(AppProperties.DiscordToken));

            await discord.StartAsync();
        }
    }
}