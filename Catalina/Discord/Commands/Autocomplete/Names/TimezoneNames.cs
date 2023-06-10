using Discord;
using Discord.Interactions;
using FuzzySharp;
using Microsoft.Extensions.DependencyInjection;
using NodaTime.TimeZones;
using Serilog.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands.Autocomplete;

public class TimezoneNames : AutocompleteHandler
{

    protected override string GetLogString(IInteractionContext context) => $"Getting timezone for {context.User}";

    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(
        IInteractionContext context,
        IAutocompleteInteraction autocompleteInteraction,
        IParameterInfo parameter,
        IServiceProvider services)
    {

        try
        {
            var value = autocompleteInteraction.Data.Current.Value as string;
            var results = new List<AutocompleteResult>();
            var timezones = TzdbDateTimeZoneSource.Default.ZoneLocations;

            results = timezones.Select(r => new AutocompleteResult
            {
                Name = r.ZoneId.Replace("/", " - ").Replace('_', ' '),
                Value = r.ZoneId
            }).ToList();

            if (string.IsNullOrEmpty(value))
                return AutocompletionResult.FromSuccess(results.Take(25));

            var names = results.Select(r => r.Name).ToList();

            var searchResults = Process.ExtractTop(query: value, choices: names, limit: 25, cutoff: 0).Select(e => e.Value).ToList();

            if (searchResults.Any())
            {
                var matches = new List<AutocompleteResult>();

                foreach (var result in searchResults)
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
}