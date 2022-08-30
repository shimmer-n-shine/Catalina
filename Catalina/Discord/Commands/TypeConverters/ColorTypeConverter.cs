using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands.TypeConverters
{
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
                if (CatalinaColours.FromName(input).Equals(Color.Default))
                {
                    var c = System.Drawing.ColorTranslator.FromHtml(input);

                    return Task.FromResult(TypeConverterResult.FromSuccess(new Color(c.R, c.G, c.B)));
                }
                else
                {
                    return Task.FromResult(TypeConverterResult.FromSuccess(CatalinaColours.FromName(input)));
                }
                
            }
            catch 
            {
                context.Interaction.RespondAsync(embed: new Utils.ErrorMessage { User = context.User, Exception = new ArgumentException() }, ephemeral: true);
                return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ParseFailed, $"`{input}` is not a valid Color Input"));
            }

            Color? result = null;

            string[] split;

            if (input.Contains(", "))
            {
                split = input.Split(", ");
            }
            else if (input.Contains(','))
            {
                split = input.Split(",");
            }
            else
            {
                split = input.Split(" ");
            }

            try
            {
                split[0] = split[0].Trim();
                split[1] = split[1].Trim();
                split[2] = split[2].Trim();
            }
            catch
            {
                context.Interaction.RespondAsync(embed: new Utils.ErrorMessage { User = context.User, Exception = new ArgumentException() }, ephemeral: true);
                return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ParseFailed, $"`{input}` is not a valid Color Input"));
            }
            

            if (!int.TryParse(split[0], out int r))
            {
                return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ParseFailed, $"`{input}` is not a valid Color Input"));
            }
            if (!int.TryParse(split[1], out int g))
            {
                return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ParseFailed, $"`{input}` is not a valid Color Input"));
            }
            if (!int.TryParse(split[2], out int b))
            {
                return Task.FromResult(TypeConverterResult.FromError(InteractionCommandError.ParseFailed, $"`{input}` is not a valid Color Input"));
            }

            result = new Color(r, g, b);

            return Task.FromResult(TypeConverterResult.FromSuccess(result.Value));
        }
    }
}