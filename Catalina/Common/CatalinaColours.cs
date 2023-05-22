using Discord;
using Humanizer;
using System.Collections.Generic;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Catalina.Common;

public static class CatalinaColours
{
    #region Darks and Lights
    /// <summary>
    /// Represents no color, or #000000;
    /// </summary>
    public static Color None { get; } = new Color(0);

    /// <summary>
    /// A vibrant blue-black color, or #121244
    /// </summary>
    public static Color Black { get; } = new Color(0x121244);

    /// <summary>
    /// A light-blue, but majority white color, or #D5D7FB
    /// </summary>
    public static Color White { get; } = new Color(0xD5D7FB);

    /// <summary>
    /// A blue-gray, or #78858E.
    /// </summary>
    public static Color Gray { get; } = new Color(0x78858E);

    /// <summary>
    /// A dark blue-gray, or #54646C.
    /// </summary>
    public static Color DarkGray { get; } = new Color(0x54646C);

    /// <summary>
    /// A light blue-gray, or #A1B2C1.
    /// </summary>
    public static Color LightGray { get; } = new Color(0xA1B2C1);

    /// <summary>
    /// Very dark blue-gray, or #313F47.
    /// </summary>
    public static Color VeryDarkGray { get; } = new Color(0x313F47);
    #endregion

    #region Discord Branding Colors
    // https://discord.com/branding

    /// <summary>
    /// Discord Blurple, or #5865F2.
    /// </summary>
    public static Color DiscordBlurple { get; } = new Color(0x5865F2);

    /// <summary>
    /// Discord Green, or #57F287.
    /// </summary>
    public static Color DiscordGreen { get; } = new Color(0x57F287);

    /// <summary>
    /// Discord Yellow, or #FEE75C.
    /// </summary>
    public static Color DiscordYellow { get; } = new Color(0xFEE75C);

    /// <summary>
    /// Discord Fuchsia, or #EB459E.
    /// </summary>
    public static Color DiscordFuchsia { get; } = new Color(0xEB459E);

    /// <summary>
    /// Discord Red, or #ED4245.
    /// </summary>
    public static Color DiscordRed { get; } = new Color(0xED4245);
    #endregion

    #region Catalina's Colour Scheme
    /// <summary>
    /// A pinky-red, or #F13472.
    /// </summary>
    public static Color Red { get; } = new Color(0xF13472);

    /// <summary>
    /// An emerald-green, or #53F46E.
    /// </summary>
    public static Color Green { get; } = new Color(0x53F46E);

    /// <summary>
    /// A purply-blue, or #5B7CEC.
    /// </summary>
    public static Color Blue { get; } = new Color(0x5B7CEC);

    /// <summary>
    /// A saffron-yellow, or #F6DD5D.
    /// </summary>
    public static Color Yellow { get; } = new Color(0xF6DD5D);

    /// <summary>
    /// A diamond-cyan, or #7AF4E6.
    /// </summary>
    public static Color Cyan { get; } = new Color(0x7AF4E6);

    /// <summary>
    /// A vibrant magenta, or #AF49F1.
    /// </summary>
    public static Color Magenta { get; } = new Color(0xAF49F1);

    /// <summary>
    /// A rustic bronze-gold, or #EFAD58.
    /// </summary>
    public static Color Gold { get; } = new Color(0xEFAD58);

    /// <summary>
    /// A cotton candy pink, or #FF91F3.
    /// </summary>
    public static Color Violet { get; } = new Color(0xFF91F3);

    /// <summary>
    /// A wood-brown, or #73482C.
    /// </summary>
    public static Color Brown { get; } = new Color(0x73482C);

    /// <summary>
    /// A lotus-lilac, or #C8A2C8.
    /// </summary>
    public static Color Lilac { get; } = new Color(0xC8A2C8);

    private static Dictionary<string, Color> _dictionaryColours;
    #endregion

    public static Color FromName(string input)
    {
        var fields = typeof(CatalinaColours).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
        foreach (var prop in fields)
        {
            if (input.ToLower().Equals(prop.Name.ToLower()))
            {
                return (Color) prop.GetValue(null);
            }
        }
        throw new System.ArgumentException("could not get colour from name");
    }
    public static Dictionary<string,Color> ToDictionary()
    {
        if (_dictionaryColours is null)
        {
            _dictionaryColours = new Dictionary<string, Color>();
            var fields = typeof(CatalinaColours).GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            foreach (var prop in fields)
            {
                _dictionaryColours.Add(prop.Name.Humanize(LetterCasing.Title), (Color)prop.GetValue(null));
            }
        }

        return _dictionaryColours;
    } 
}
