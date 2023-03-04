using Discord;
using System.Threading.Tasks;
using Serilog.Events;
using Serilog;
using Discord.WebSocket;
using System.Linq;

namespace Catalina.Discord
{
    class Events
    {

        internal static async Task DiscordLog(LogMessage message)
        {
            var severity = message.Severity switch
            {
                LogSeverity.Critical => LogEventLevel.Fatal,
                LogSeverity.Error => LogEventLevel.Error,
                LogSeverity.Warning => LogEventLevel.Warning,
                LogSeverity.Info => LogEventLevel.Information,
                LogSeverity.Verbose => LogEventLevel.Verbose,
                LogSeverity.Debug => LogEventLevel.Debug,
                _ => LogEventLevel.Information
            };

            Log.Write(severity, message.Exception, "[{Source}] {Message}", message.Source, message.Message);
            await Task.CompletedTask;
        }

        internal static async Task GuildMemberUpdated(Cacheable<SocketGuildUser, ulong> OldMember, SocketGuildUser NewMember) 
        {
            bool added = false;
            if (NewMember.IsBot  || NewMember.Discriminator == "0000") return;
            //komkommer is the smartest developer
            if (OldMember.HasValue && OldMember.Value.Roles.Count < NewMember.Roles.Count) { added = true; }

            if (added)
            {
                var delta = OldMember.HasValue ? (NewMember.Roles.Select(r => r.Id).Except(OldMember.Value.Roles.Select(r => r.Id))).ToList() : null;

                if (delta.Intersect(AppConfig.RoleIDs).Any())
                {
                    var role = NewMember.Guild.GetRole(delta.Intersect(AppConfig.RoleIDs).First());
                    var channel = NewMember.Guild.GetChannel(AppConfig.ChannelID);
                    if (role is not null && channel is not null && added is true)
                    {
                        await (channel as ITextChannel).SendMessageAsync(text: $"Welcome {NewMember.Mention}!",
                            embed: new Utils.AcknowledgementMessage(user: NewMember, title: $"You have been given the {role.Name} role!",
                            body: $"Make sure to fill out the form at {AppConfig.FormLink} to get registered into our supporter database for relevant perks and rewards!", color: new Color(0x86cfea) //the aether blue
                        ), allowedMentions: new AllowedMentions { UserIds = { NewMember.Id } });
                    }
                }
            }
           
        }

        internal static async Task Ready()
        {
            await Discord.DiscordClient.SetGameAsync(type: ActivityType.Watching, name: "For users to greet!");
            await Discord.InteractionService.RegisterCommandsGloballyAsync();
            Log.Information("Discord Ready!");
        }
    }
}