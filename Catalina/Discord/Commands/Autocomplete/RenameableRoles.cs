using Catalina.Database;
using Catalina.Discord.Commands.Preconditions;
using Discord;
using Discord.Interactions;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands.Autocomplete
{
    public class RoleStylisation : AutocompleteHandler
    {
        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services
        )
        {
            await using var database = new DatabaseContextFactory().CreateDbContext();

            try
            {
                var value = autocompleteInteraction.Data.Current.Value as string;

                var results = new List<AutocompleteResult>();

                //var preliminaryGuildRoleResults = database.GuildProperties.Include(g => g.Roles).AsNoTracking().SelectMany(g => g.Roles).Where(r => r.IsRenamabale).Select(r => r.ID).ToList();
                var preliminaryGuildRoleResults = database.GuildProperties.AsNoTracking().SelectMany(g => g.Roles).Where(r => r.IsRenamabale).Select(r => r.ID).ToList();

                var preliminaryUserRoleResults = (context.User as IGuildUser).RoleIds;

                results = preliminaryGuildRoleResults.Intersect(preliminaryUserRoleResults).Select(r => new AutocompleteResult {
                    Name = context.Guild.GetRole(r).Name,
                    Value = r.ToString()
                }).ToList();

                if (string.IsNullOrEmpty(value))
                    return AutocompletionResult.FromSuccess(results.Take(25));

                var names = results.Select(r => r.Name).ToList();


                Dictionary<string, int> orderedResults = new();

                names.ForEach(x =>
                {
                    var confidence = FuzzyString.ComparisonMetrics.LevenshteinDistance(value, x);
                    orderedResults.Add(x, confidence);
                });

                var searchResults = orderedResults.OrderBy(x => x.Value);

                if (searchResults.Any())
                {
                    var matches = new List<AutocompleteResult>();

                    foreach (var result in searchResults)
                    {
                        matches.Add(results.FirstOrDefault(z => z.Name == result.Key));
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
                NLog.LogManager.GetCurrentClassLogger().Error(ex, ex.Message);

                return AutocompletionResult.FromError(ex);
            }
        }

        protected override string GetLogString(IInteractionContext context) => $"Getting roles for {context.User}";
    }
}
