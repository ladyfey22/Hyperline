namespace Celeste.Mod.Hyperline
{
    using Microsoft.Xna.Framework;
    using Monocle;
    using System.Collections.Generic;
    using System.IO;
    using System.Reflection;
    using System.Xml.Linq;

    /// <summary>
    /// Interface for all hair types.
    /// </summary>
    /// <remarks>
    /// Check GradiantHair, SolidHair, and PatternHair for example implementations.
    /// </remarks>
    public abstract class IHairType
    {
        private static readonly FieldInfo HairflashtimerField = typeof(Player).GetField("hairFlashTimer", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void ReadColorElement(XElement element, string name, ref HSVColor color)
        {
            XElement colorElement = element.Element(name);
            if (colorElement != null)
            {
                color.FromString((string)colorElement);
            }
        }

        public static float GetHairFlashTimer(Player player)
        {
            float flashTimer = 0.0f;
            if (HairflashtimerField != null)
            {
                object obj = HairflashtimerField.GetValue(player);
                if (obj != null)
                {
                    flashTimer = (float)obj;
                }
            }
            return flashTimer;
        }

        private static int lastDashes;
        private static float hairFlashTimer;

        public static bool IsFlash() => hairFlashTimer > 0;

        public static Color LerpFlash(Color c) => Color.Lerp(c, Player.FlashHairColor, (float)System.Math.Sin((double)(hairFlashTimer / 0.12f * MathHelper.Pi)));

        /// <summary>
        /// Function to get the display name of a hair type.
        /// </summary>
        /// <returns> Dialog key to the display name of this hair type. </returns>
        public abstract string GetHairName();

        /// <summary>
        /// Create a new copy of the IHairType with the same values.
        /// </summary>
        /// <returns>A clone of the IHairType.</returns>
        public abstract IHairType Clone();

        /// <summary>
        /// Gets the unique id of the hair type.
        /// </summary>
        /// <remarks>
        /// Unique id is expected in the form of "Modname_HairTypeName"
        /// </remarks>
        /// <returns> The unique id of the hair type. </returns>
        public abstract string GetId();

        /// <summary>
        /// Gets the unique integer hash of the id.
        /// </summary>
        /// <remarks>
        /// Hashing is done through the Hashing.FNV1Hash() static function.
        /// </remarks>
        /// <returns> A unique integer hash of the id. </returns>
        public abstract uint GetHash();

        /// <summary>
        /// Calculates the current color returned by the hair type.
        /// </summary>
        /// <param name="phase"> 
        /// A parameter between 0-1.
        /// representing the position along the length of the hair (accounting for speed).
        /// </param>
        /// <param name="colorOrig">
        /// The original color of the hair.
        /// </param>
        /// <returns> The color of the section of the hair. </returns>
        public abstract Color GetColor(Color colorOrig, float phase);

        /// <summary>
        /// Reads the hair type from a stream. This is for legacy support, and is not required.
        /// </summary>
        /// <param name="reader"> The stream to read from. </param>
        /// <param name="version"> The version array (size 3), form {Major,Minor,Sub} </param>
        public abstract void Read(BinaryReader reader, byte[] version);

        public abstract void Read(XElement element);

        /// <summary>
        /// Writes the hair type to a stream. This is for legacy support, and is not required.
        /// </summary>
        /// <param name="writer"> The stream to write to. </param>
        public abstract void Write(BinaryWriter writer);

        public abstract void Write(XElement element);

        /// <summary>
        /// Creates a new instance of this hair type.
        /// </summary>
        /// <returns> An instance of the hair type. </returns>
        public abstract IHairType CreateNew();

        /// <summary>
        /// Creates a new instance of this hair type, depending on dash count.
        /// </summary>
        /// <remarks>
        /// Really only meant to be able to include default colors in a hair type.
        /// </remarks>
        /// <param name="i"> The dash count.</param>
        /// <returns> An instance of the hair type. </returns>
        public abstract IHairType CreateNew(int i);

        /// <summary>
        /// Loads the hair type from a string.
        /// </summary>
        /// <remarks>
        /// Provided as a means of loading from ahorn attributes.
        /// Ensure any characters expected can be entered into ahorn.
        /// In addition, ensure the format does not use ';', as this is reserved.
        /// </remarks>
        /// <param name="str"> The string to load from. </param>
        /// <returns> A new instance of the hair type, constructed from information in the string.</returns>
        public abstract IHairType CreateNew(string str);

        /// <summary>
        /// Constructs a menu for the options of the hair type.
        /// </summary>
        /// <remarks>
        /// Don't add string editors if inGame is true, it'll cause everest to crash.
        /// </remarks>
        /// <param name="menu"> Top menu. </param>
        /// <param name="inGame"> Whether or not this is the in game options menu. </param>
        /// <returns> A list of menu option items to be added. </returns>
        public abstract List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame);

        /// <summary>
        /// Renders the hair, allowing for custom rendering.
        /// </summary>
        /// <param name="orig">The default function for hair rendering.</param>
        /// <param name="self">The player's hair</param>
        /// <remarks>
        /// It is reccomended to look at the default hair rendering code before writing this.
        /// To keep compatability, use the hair color and texture functions in the PlayerHair class.
        /// </remarks>
        public virtual void Render(On.Celeste.PlayerHair.orig_Render orig, PlayerHair self) => orig(self);

        /// <summary>
        /// Updates the hair, allowing for custom physics.
        /// </summary>
        /// <param name="orig">The default function for hair updating.</param>
        /// <param name="self">The player's hair.</param>
        /// <remarks>
        /// It is reccomended to look at the default hair after update before writing this.
        /// </remarks>
        public virtual void AfterUpdate(On.Celeste.PlayerHair.orig_AfterUpdate orig, PlayerHair self)
        {
            Player player = self.EntityAs<Player>();
            if (!(!Hyperline.Settings.DoFeatherColor && player.StateMachine.State == 19))
            {
                player.Hair.Color = Hyperline.GetCurrentColor(Hyperline.Instance.LastColor, player.Dashes, 0);
            }

            orig(self);
        }

        public virtual void PlayerUpdate(Color lastColor, Player player)
        {
            if (player != null)
            {
                player.OverrideHairColor = Hyperline.GetCurrentColor(Hyperline.Instance.LastColor, player.Dashes, 0);
                Hyperline.Instance.MaddyCrownSprite = null;
            }
        }

        public virtual void UpdateHair(On.Celeste.Player.orig_UpdateHair orig, Player self, bool applyGravity)
        {
            if (lastDashes != self.Dashes)
            {
                hairFlashTimer = 0.12f;
            }

            if (hairFlashTimer > 0f)
            {
                hairFlashTimer -= Engine.DeltaTime;
            }

            lastDashes = self.Dashes;
            orig(self, applyGravity);
        }
    }
}
