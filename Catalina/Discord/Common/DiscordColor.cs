using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//original code pls dont steal
namespace Catalina.Discord
{
    public static class DiscordColor
    {
        #region Black and White
        /// <summary>
        /// Represents no color, or integer 0;
        /// </summary>
        public static Color None { get; } = new Color(0);

        /// <summary>
        /// A near-black color. Due to API limitations, the color is #010101, rather than #000000, as the latter is treated as no color.
        /// </summary>
        public static Color Black { get; } = new Color(0x010101);

        /// <summary>
        /// White, or #FFFFFF.
        /// </summary>
        public static Color White { get; } = new Color(0xFFFFFF);

        /// <summary>
        /// Gray, or #808080.
        /// </summary>
        public static Color Gray { get; } = new Color(0x808080);

        /// <summary>
        /// Dark gray, or #A9A9A9.
        /// </summary>
        public static Color DarkGray { get; } = new Color(0xA9A9A9);

        /// <summary>
        /// Light gray, or #808080.
        /// </summary>
        public static Color LightGray { get; } = new Color(0xD3D3D3);

        // dev-approved
        /// <summary>
        /// Very dark gray, or #666666.
        /// </summary>
        public static Color VeryDarkGray { get; } = new Color(0x666666);
        #endregion

        #region Discord branding colors
        // https://discord.com/branding

        /// <summary>
        /// Discord Blurple, or #7289DA.
        /// </summary>
        public static Color Blurple { get; } = new Color(0x7289DA);

        /// <summary>
        /// Discord Grayple, or #99AAB5.
        /// </summary>
        public static Color Grayple { get; } = new Color(0x99AAB5);

        /// <summary>
        /// Discord Dark, But Not Black, or #2C2F33.
        /// </summary>
        public static Color DarkButNotBlack { get; } = new Color(0x2C2F33);

        /// <summary>
        /// Discord Not QuiteBlack, or #23272A.
        /// </summary>
        public static Color NotQuiteBlack { get; } = new Color(0x23272A);
        #endregion

        #region Other colors
        /// <summary>
        /// Red, or #FF0000.
        /// </summary>
        public static Color Red { get; } = new Color(0xFF0000);

        /// <summary>
        /// Dark red, or #7F0000.
        /// </summary>
        public static Color DarkRed { get; } = new Color(0x7F0000);

        /// <summary>
        /// Green, or #00FF00.
        /// </summary>
        public static Color Green { get; } = new Color(0x00FF00);

        /// <summary>
        /// Dark green, or #007F00.
        /// </summary>
        public static Color DarkGreen { get; } = new Color(0x007F00);

        /// <summary>
        /// Blue, or #0000FF.
        /// </summary>
        public static Color Blue { get; } = new Color(0x0000FF);

        /// <summary>
        /// Dark blue, or #00007F.
        /// </summary>
        public static Color DarkBlue { get; } = new Color(0x00007F);

        /// <summary>
        /// Yellow, or #FFFF00.
        /// </summary>
        public static Color Yellow { get; } = new Color(0xFFFF00);

        /// <summary>
        /// Cyan, or #00FFFF.
        /// </summary>
        public static Color Cyan { get; } = new Color(0x00FFFF);

        /// <summary>
        /// Magenta, or #FF00FF.
        /// </summary>
        public static Color Magenta { get; } = new Color(0xFF00FF);

        /// <summary>
        /// Teal, or #008080.
        /// </summary>
        public static Color Teal { get; } = new Color(0x008080);

        // meme
        /// <summary>
        /// Aquamarine, or #00FFBF.
        /// </summary>
        public static Color Aquamarine { get; } = new Color(0x00FFBF);

        /// <summary>
        /// Gold, or #FFD700.
        /// </summary>
        public static Color Gold { get; } = new Color(0xFFD700);

        // To be fair, you have to have a very high IQ to understand Goldenrod .
        // The tones are extremely subtle, and without a solid grasp of artistic
        // theory most of the beauty will go over a typical painter's head.
        // There's also the flower's nihilistic style, which is deftly woven
        // into its characterization - it's pollinated by the Bombus cryptarum
        // bumblebee, for instance. The fans understand this stuff; they have
        // the intellectual capacity to truly appreciate the depth of this
        // flower, to realize that it's not just a color - it says something
        // deep about LIFE. As a consequence people who dislike Goldenrod truly
        // ARE idiots - of course they wouldn't appreciate, for instance, the
        // beauty in the bumblebee species' complex presence in the British Isles,
        // which is cryptically explained by Turgenev's Russian epic Fathers and
        // Sons I'm blushing right now just imagining one of those addlepated
        // simpletons scratching their heads in confusion as nature's genius
        // unfolds itself on their computer screens. What fools... how I pity them.
        // 😂 And yes by the way, I DO have a goldenrod tattoo. And no, you cannot
        // see it. It's for the ladies' eyes only- And even they have to
        // demonstrate that they're within 5 IQ points of my own (preferably lower) beforehand.
        /// <summary>
        /// Goldenrod, or #DAA520.
        /// </summary>
        public static Color Goldenrod { get; } = new Color(0xDAA520);

        // emzi's favourite
        /// <summary>
        /// Azure, or #007FFF.
        /// </summary>
        public static Color Azure { get; } = new Color(0x007FFF);

        /// <summary>
        /// Rose, or #FF007F.
        /// </summary>
        public static Color Rose { get; } = new Color(0xFF007F);

        /// <summary>
        /// Spring green, or #00FF7F.
        /// </summary>
        public static Color SpringGreen { get; } = new Color(0x00FF7F);

        /// <summary>
        /// Chartreuse, or #7FFF00.
        /// </summary>
        public static Color Chartreuse { get; } = new Color(0x7FFF00);

        /// <summary>
        /// Orange, or #FFA500.
        /// </summary>
        public static Color Orange { get; } = new Color(0xFFA500);

        /// <summary>
        /// Purple, or #800080.
        /// </summary>
        public static Color Purple { get; } = new Color(0x800080);

        /// <summary>
        /// Violet, or #EE82EE.
        /// </summary>
        public static Color Violet { get; } = new Color(0xEE82EE);

        /// <summary>
        /// Brown, or #A52A2A.
        /// </summary>
        public static Color Brown { get; } = new Color(0xA52A2A);

        // meme
        /// <summary>
        /// Hot pink, or #FF69B4
        /// </summary>
        public static Color HotPink { get; } = new Color(0xFF69B4);

        /// <summary>
        /// Lilac, or #C8A2C8.
        /// </summary>
        public static Color Lilac { get; } = new Color(0xC8A2C8);

        /// <summary>
        /// Cornflower blue, or #6495ED.
        /// </summary>
        public static Color CornflowerBlue { get; } = new Color(0x6495ED);

        /// <summary>
        /// Midnight blue, or #191970.
        /// </summary>
        public static Color MidnightBlue { get; } = new Color(0x191970);

        /// <summary>
        /// Wheat, or #F5DEB3.
        /// </summary>
        public static Color Wheat { get; } = new Color(0xF5DEB3);

        /// <summary>
        /// Indian red, or #CD5C5C.
        /// </summary>
        public static Color IndianRed { get; } = new Color(0xCD5C5C);

        /// <summary>
        /// Turquoise, or #30D5C8.
        /// </summary>
        public static Color Turquoise { get; } = new Color(0x30D5C8);

        /// <summary>
        /// Sap green, or #507D2A.
        /// </summary>
        public static Color SapGreen { get; } = new Color(0x507D2A);

        // meme, specifically bob ross
        /// <summary>
        /// Phthalo blue, or #000F89.
        /// </summary>
        public static Color PhthaloBlue { get; } = new Color(0x000F89);

        // meme, specifically bob ross
        /// <summary>
        /// Phthalo green, or #123524.
        /// </summary>
        public static Color PhthaloGreen { get; } = new Color(0x123524);

        /// <summary>
        /// Sienna, or #882D17.
        /// </summary>
        public static Color Sienna { get; } = new Color(0x882D17);
        #endregion
    }
}
