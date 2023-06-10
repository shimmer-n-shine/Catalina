﻿using Catalina.Database;
using Discord;
using Discord.Interactions;
using FuzzySharp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands.Autocomplete;

public class RenameableRoles : AutocompleteHandler
{
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services
    )
    {
        using var database = services.GetRequiredService<DatabaseContext>();

        try
        {
            var value = autocompleteInteraction.Data.Current.Value as string;

            var results = new List<AutocompleteResult>();

            var preliminaryGuildRoleResults = database.Guilds.Include(g => g.Roles).AsNoTracking().Where(g => g.ID == context.Guild.Id).SelectMany(g => g.Roles).Where(r => r.IsRenamabale).Select(r => r.ID).ToList();

            var preliminaryUserRoleResults = (context.User as IGuildUser).RoleIds;

            results = preliminaryGuildRoleResults.Intersect(preliminaryUserRoleResults).Select(r => new AutocompleteResult
            {
                Name = context.Guild.GetRole(r).Name,
                Value = r.ToString()
            }).ToList();

            if (string.IsNullOrEmpty(value))
                return AutocompletionResult.FromSuccess(results.Take(25));

            var names = results.Select(r => r.Name).ToList();

            var searchResults = Process.ExtractTop(query: value, choices: names, limit: 25, cutoff: 0);

            if (searchResults.Any())
            {
                var cutResults = searchResults.Where(s => s.Score >= searchResults.First().Score / 2).Select(e => e.Value).ToList();

                var matches = new List<AutocompleteResult>();

                foreach (var result in cutResults)
                {
                    matches.Add(results.FirstOrDefault(z => z.Name == result));
                }

                var matchCollection = matches.Count > 25 ? matches.Take(25) : matches;

                return AutocompletionResult.FromSuccess(matchCollection);
            }
            else
            {
                return AutocompletionResult.FromError(InteractionCommandError.Unsuccessful, "Couldn't find any results");
            }
        }
        catch (Exception ex)
        {
            services.GetRequiredService<Logger>().Error(ex, ex.Message);

            return AutocompletionResult.FromError(ex);
        }
    }

    protected override string GetLogString(IInteractionContext context) => $"Getting roles for {context.User}";
}
