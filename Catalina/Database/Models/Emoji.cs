using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using DiscordNET = Discord;

namespace Catalina.Database.Models;
public class Emoji
{
    [Key] public string NameOrID { get; set; }
    public EmojiType Type { get; set; }
    public enum EmojiType : byte { Internal, External };

    public static async Task<Emoji> ParseAsync(DiscordNET.IEmote emote, DiscordNET.IGuild guild)
    {
        if (emote is DiscordNET.Emote externalEmote)
        {
            try
            {
                await guild.GetEmoteAsync(externalEmote.Id);
                return new Emoji { Type = EmojiType.External, NameOrID = externalEmote.Id.ToString() };
            }
            catch
            {
                throw new System.ArgumentException("External emoji invalid or not from current guild");
            }

        }
        else
        {
            return emote is DiscordNET.Emoji internalEmote
                ? new Emoji { Type = EmojiType.Internal, NameOrID = EmojiToolkit.Emoji.Shortcode(internalEmote.Name) }
                : throw new System.ArgumentException("Invalid emote provided.");
        }
    }
    public static async Task<Emoji> ParseAsync(string emoji, DiscordNET.IGuild guild)
    {

        try
        {
            return await ParseAsync(DiscordNET.Emoji.Parse(emoji), guild);
        }
        catch
        {
            try
            {
                var emote = DiscordNET.Emote.Parse(emoji);
                try
                {
                    return await ParseAsync(await guild.GetEmoteAsync(emote.Id), guild);
                }
                catch
                {
                    throw new System.ArgumentException("emote is not from this guild");
                }
            }
            catch
            {
                throw new System.ArgumentException("did not pass a valid emoji");
            }
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
                return new DiscordNET.Emoji(EmojiToolkit.Emoji.Get(emoji.NameOrID).Raw);
            }
            catch
            {
                throw new System.ArgumentException("Internal emoji invalid");
            }
        }
    }
}
