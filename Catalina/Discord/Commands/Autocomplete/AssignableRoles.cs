﻿using Catalina.Database;
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
    public class AssignableRoles : AutocompleteHandler
    {

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services
        )
        {
            using var database = new DatabaseContextFactory().CreateDbContext();

            try
            {
                var value = autocompleteInteraction.Data.Current.Value as string;

                var results = new List<AutocompleteResult>();
                var userRoles = (context.User as IGuildUser).RoleIds.Select(r => context.Guild.GetRole(r)).Where(r => r.Permissions.ManageRoles || r.Permissions.Administrator);
                if (context.Guild.OwnerId == context.User.Id) userRoles = context.Guild.Roles;
                var highestUserRole = userRoles.OrderByDescending(r => r.Position).First();
                var botRoles = (await context.Guild.GetCurrentUserAsync()).RoleIds.Select(r => context.Guild.GetRole(r)).Where(r => r.Permissions.ManageRoles || r.Permissions.Administrator && r.Position < highestUserRole.Position);
                var highestBotRole = botRoles.OrderByDescending(r => r.Position).First();
                var preliminaryRoleResults = context.Guild.Roles.Where(r => r.Position < highestBotRole.Position);

                results = preliminaryRoleResults.Select(r => new AutocompleteResult {
                    Name = r.Name,
                    Value = r.Id.ToString()
                }).ToList();

                if (string.IsNullOrEmpty(value))
                    try
                    {
                        return AutocompletionResult.FromSuccess(results.Take(25));
                    }
                    catch
                    {

                    }
                    

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