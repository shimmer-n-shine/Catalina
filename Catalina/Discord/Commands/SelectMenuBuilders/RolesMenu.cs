using Catalina.Database;
using Catalina.Database.Models;
using Discord;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Data;
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
        var botRoles = (await Guild.GetCurrentUserAsync()).RoleIds.Select(r => Guild.GetRole(r)).Where(r => r.Permissions.ManageRoles || (r.Permissions.Administrator && r.Position < highestUserRole.Position));
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

    public List<SelectMenuOptionBuilder> GetOptions(int pageNumber)
    {
        pageNumber = pageNumber < 0 ? 0 : pageNumber;
        var roles = GetRoles().Result;
        var rolesList = roles.ToList();
        List<KeyValuePair<IRole, Role>> currentRoles = new List<KeyValuePair<IRole, Role>>();
        int pageCount = 0;

        //first page 24, last page 24, all in between pages 23

        //if count < 24, then all can be added
        //if count > 24 but < 48, then 24 per page
        //if count > 48, then 24 on first and last page

        //to get middle pages: subtract 48, then check 23s.
        int startIndex = 0;
        int endIndex = 23;

        if (rolesList.Count <= 24) 
        {
            //less than 24 objects
            pageCount = 1;

            currentRoles = rolesList;
        }
        else if (rolesList.Count <= 48)
        {
            pageCount = 2;
            pageNumber = pageNumber > pageCount ? 0 : pageNumber;
            //less than 48 objects
            startIndex = pageNumber * 24;
            endIndex = Math.Min(startIndex + 24, rolesList.Count);
        }
        else
        {

            //more than 48 items
            int rolesExcludingEnds = rolesList.Count - 48;
            pageCount = (int)(MathF.Ceiling(rolesExcludingEnds / 23f) + 2);
            pageNumber = pageNumber > pageCount - 1 ? 0 : pageNumber;
            //first page
            if (pageNumber == 0)
            {
                startIndex = 0;
                endIndex = 24;
            }
            //last page
            else if (pageNumber == pageCount - 1)
            {
                startIndex = rolesList.Count - 23;
                endIndex = rolesList.Count;
            }
            //middle pages
            else
            {
                startIndex = pageNumber * 23 + 1;
                endIndex = startIndex + 23;
            }
        }
        currentRoles = rolesList.Take(new Range(startIndex, endIndex)).ToList();

        var optionsPage = new List<SelectMenuOptionBuilder>();
        //add back arrow
        if (currentRoles.Count > 1 && pageNumber != 0)
        {
            optionsPage.Add(new SelectMenuOptionBuilder()
            {
                Label = "Previous Page",
                Value = "previousPage",
                Emote = DiscordNET.Emoji.Parse(EmojiToolkit.Emoji.Get(":arrow_left:").Raw)
            });
        }
           
        foreach (var role in currentRoles)
        {
            optionsPage.Add(new SelectMenuOptionBuilder()
            {
                Label = role.Key.Name,
                Value = role.Key.Id.ToString(),
                Emote = role.Value is not null ? DiscordNET.Emoji.Parse(EmojiToolkit.Emoji.Get(":pencil:").Raw) : DiscordNET.Emoji.Parse(EmojiToolkit.Emoji.Get(":page_facing_up:").Raw)
            });
        }

        //add forward arrow
        if (currentRoles.Count > 1 && pageNumber != pageCount - 1)
        {
            optionsPage.Add(new SelectMenuOptionBuilder()
            {
                Label = "Next Page",
                Value = "nextPage",
                Emote = DiscordNET.Emoji.Parse(EmojiToolkit.Emoji.Get(":arrow_right:").Raw)
            });
        }

        return optionsPage;
    }

    public SelectMenuBuilder ToSelectMenuBuilder(int pageNumber)
    {
        return new SelectMenuBuilder(customId: ID, placeholder: Placeholder, options: GetOptions(pageNumber));
    }
}
