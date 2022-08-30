using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DiscordNET = Discord;

namespace Catalina.Database.Models;
public class Emoji
{
    [Key] public string NameOrID { get; set; }
    public EmojiType Type { get; set; }
    public enum EmojiType : byte { Internal, External };

    public static Emoji Parse(DiscordNET.IEmote emote, DiscordNET.IGuild guild)
    {
        if (emote is DiscordNET.Emote externalEmote)
        {
            try
            {
                guild.GetEmoteAsync(externalEmote.Id);
                return new Emoji { Type = EmojiType.External, NameOrID = externalEmote.Id.ToString() };
            }
            catch
            {
                throw new System.ArgumentException("External emoji invalid or not from current guild");
            }

        }
        else if (emote is DiscordNET.Emoji internalEmote)
        {
            return new Emoji { Type = EmojiType.Internal, NameOrID = EmojiOne.EmojiOne.ToShort(internalEmote.Name) };
        }
        else
        {
            throw new System.ArgumentException("Invalid emote provided.");
        }
    }
    public static async Task<DiscordNET.IEmote> ToEmoteAsync(Emoji emoji, DiscordNET.IGuild guild)
    {
        if (emoji.Type == EmojiType.External)
        {
            try
            {
                var emote = await guild.GetEmoteAsync(ulong.Parse(emoji.NameOrID));
                return emote;
            }
            catch
            {
                throw new System.ArgumentException("External emoji invalid or not from current guild");
            }
        }
        else
        {
            try
            {
                return new DiscordNET.Emoji(EmojiOne.EmojiOne.ShortnameToUnicode(emoji.NameOrID));
            }
            catch
            {
                throw new System.ArgumentException("Internal emoji invalid");
            }
        }
    }
}
