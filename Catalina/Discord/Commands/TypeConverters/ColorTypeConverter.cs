using Catalina.Common;
using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands.TypeConverters;

    public class ColorTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(Type type) => typeof(Color).IsAssignableFrom(type);

        public override ApplicationCommandOptionType GetDiscordType() => ApplicationCommandOptionType.String;

        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            var input = option.Value as string;

            if (string.IsNullOrWhiteSpace(input)) return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ParseFailed, "Input is empty"));

            if (input.StartsWith("#") || (long.TryParse(input, System.Globalization.NumberStyles.HexNumber, null, out _) && input.Length == 6))
            {
                if (!input.StartsWith('#')) input = '#' + input;
                System.Drawing.Color col = System.Drawing.ColorTranslator.FromHtml(input);

                return Task.FromResult(TypeConverterResult.FromSuccess(new Color(col.R, col.G, col.B)));
            }

            try
            {
                return Task.FromResult(TypeConverterResult.FromSuccess(CatalinaColours.FromName(input)));
                
            }
            catch 
            {
                context.Interaction.RespondAsync(embed: new Utils.ErrorMessage (user: context.User) { Exception = new ArgumentException() }, ephemeral: true);
                return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ParseFailed, $"`{input}` is not a valid Color Input"));
            }
        }
    }