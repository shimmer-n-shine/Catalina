using Catalina.Database;
using Catalina.Database.Models;
using Discord;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DiscordNET = Discord;

namespace Catalina.Discord.Commands.SelectMenuBuilders;
public class RolesMenu : ISelectMenu
{
    private DatabaseContext _database;
    public string ID { get; set; } = "role_menu";
    public string Placeholder { get; set; } = "Pick the role you'd like the configure!";

    public IGuild Guild { get; set; }
    public IGuildUser User { get; set; }

    public RolesMenu(DatabaseContext dbContext, IGuild guild, IGuildUser user)
    {
        _database = dbContext;
        Guild = guild;
        User = user;
    }

    private async Task<Dictionary<IRole, Role>> GetRoles()
    {
        var userRoles = User.RoleIds.Select(r => Guild.GetRole(r)).Where(r => r.Permissions.ManageRoles || r.Permissions.Administrator);
        if (Guild.OwnerId == User.Id) userRoles = Guild.Roles;
        var highestUserRole = userRoles.OrderByDescending(r => r.Position).First();
        var botRoles = (await Guild.GetCurrentUserAsync()).RoleIds.Select(r => Guild.GetRole(r)).Where(r => r.Permissions.ManageRoles || r.Permissions.Administrator && r.Position < highestUserRole.Position);
        var highestBotRole = botRoles.OrderByDescending(r => r.Position).First();
        var roles = Guild.Roles.Where(r => r.Position < highestBotRole.Position);
        var dbRoles = new List<Role>();

        Dictionary<IRole, Role> rolePairs = new Dictionary<IRole, Role>();

        foreach (var role in roles)
        {
            Role dbRole = _database.Roles.FirstOrDefault(r => r.ID == role.Id);
            rolePairs.Add(role, dbRole);
        }
        return rolePairs;
    }
    public List<SelectMenuOptionBuilder> Options
    {
        get
        {
            var options = new List<SelectMenuOptionBuilder>();
            var roles = GetRoles().Result;
            var editEmoji = DiscordNET.Emoji.Parse(EmojiToolkit.Emoji.Get(":gear:").Raw);

            foreach (var role in roles)
            {

                options.Add(new SelectMenuOptionBuilder()
                {
                    Label = role.Key.Name,
                    Value = role.Key.Id.ToString(),
                    Emote = role.Value is not null ? editEmoji : null
                });
            }
            return options;
        }
    }

    public SelectMenuBuilder ToSelectMenuBuilder()
    {
        return new SelectMenuBuilder(customId: ID, placeholder: Placeholder, options: Options);
    }
}
