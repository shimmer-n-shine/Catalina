using Discord;
using Discord.Interactions;
using System;
using System.Threading.Tasks;

namespace Catalina.Discord.Commands.TypeConverters
{
    public class NullableBoolTypeConverter : TypeConverter
    {
        public override bool CanConvertTo(Type type) => typeof(bool?).IsAssignableFrom(type);

        public override ApplicationCommandOptionType GetDiscordType() => ApplicationCommandOptionType.Boolean;

        public override Task<TypeConverterResult> ReadAsync(IInteractionContext context, IApplicationCommandInteractionDataOption option, IServiceProvider services)
        {
            var input = option.Value as bool?;
            return Task.FromResult(TypeConverterResult.FromSuccess(input));
        }
    }
}
