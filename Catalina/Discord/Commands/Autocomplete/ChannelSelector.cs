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

    public class ChannelSelector : AutocompleteHandler
    {

        public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
            IInteractionContext context,
            IAutocompleteInteraction autocompleteInteraction,
            IParameterInfo parameter,
            IServiceProvider services
        )
        {
            using var database = new DatabaseContextFactory().CreateDbContext();

            if (!(await new RequirePrivilege(AccessLevel.Administrator).CheckRequirementsAsync(context, null, services)).IsSuccess)
            {
                return AutocompletionResult.FromError(InteractionCommandError.Unsuccessful, "Insufficient Permission");
            }

            try
            {
                var value = autocompleteInteraction.Data.Current.Value as string;

                var results = new List<AutocompleteResult>();

                var channels = (await context.Guild.GetChannelsAsync()).Where(c => c.GetChannelType() == ChannelType.Text);
                foreach (var channel in channels)
                {
                    results.Add(new AutocompleteResult
                    {
                        Name = channel.Name,
                        Value = channel.Id.ToString()
                    });
                }

                if (string.IsNullOrEmpty(value))
                    return AutocompletionResult.FromSuccess(results.Take(5));

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

                    var matchCollection = matches.Count() > 4 ? matches.Take(4) : matches;
                    var None = new AutocompleteResult("None", string.Empty);

                    matchCollection = matchCollection.Append(None);

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

        protected override string GetLogString(IInteractionContext context) => $"Getting channels for {context.Guild}";
    }
}
