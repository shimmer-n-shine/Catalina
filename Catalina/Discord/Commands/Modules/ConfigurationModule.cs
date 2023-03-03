using Catalina.Discord.Commands.Preconditions;
using Catalina.Discord.Common;
using Discord;
using Discord.Interactions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands.Modules
{
    [RequirePrivilege(AccessLevel.Administrator)]
    [DefaultMemberPermissions(PermissionConstants.Administrator)]
    [Group("config", "Guild configurations")]
    public class ConfigurationModule : InteractionModuleBase
    {
        [Group("greeter", "Configuration for greeter")]
        public class GreeterConfiguration : InteractionModuleBase
        {
            [SlashCommand("channel", "Set channel for greeter")]
            public async Task SetGreeterChannel([Summary("Channel")][ChannelTypes(ChannelType.Text)] IChannel channel = null)
            {
                AppConfig.ChannelID = channel.Id;
                await RespondAsync(embed: new Utils.AcknowledgementMessage(user: Context.User));
            }
            [SlashCommand("removerole", "remove role used by greeter")]
            public async Task RemoveGreeterRole([Summary("Role")] IRole role)
            {
                var newIDs = new List<ulong>(AppConfig.RoleIDs);
                if (newIDs.Contains(role.Id))
                {
                    newIDs.Remove(role.Id);
                    AppConfig.RoleIDs = newIDs.ToArray();
                    await RespondAsync(embed: new Utils.AcknowledgementMessage(user: Context.User));
                }
                else
                {
                    await RespondAsync(embed: new Utils.ErrorMessage(user: Context.User, exception: new Exceptions.InvalidRole("the role you specified could not be found.")));
                }
            }
            [SlashCommand("addrole", "add role used by greeter")]
            public async Task AddGreeterRole([Summary("Role")] IRole role)
            {
                var newIDs = new List<ulong>(AppConfig.RoleIDs)
                {
                    role.Id
                };
                AppConfig.RoleIDs = newIDs.ToArray();
                await RespondAsync(embed: new Utils.AcknowledgementMessage(user: Context.User));
            }
        }
    }
}
