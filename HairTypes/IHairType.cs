using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace Celeste.Mod.Hyperline
{
    /// <summary>
    /// Interface for all hair types.
    /// </summary>
    /// <remarks>
    /// Check GradiantHair, SolidHair, and PatternHair for example implementations.
    /// </remarks>
    public interface IHairType
    {
        /// <summary>
        /// Function to get the display name of a hair type.
        /// </summary>
        /// <returns> Dialog key to the display name of this hair type. </returns>
        string GetHairName();

        /// <summary>
        /// Gets the unique id of the hair type.
        /// </summary>
        /// <remarks>
        /// Unique id is expected in the form of "Modname_HairTypeName"
        /// </remarks>
        /// <returns> The unique id of the hair type. </returns>
        string GetId();

        /// <summary>
        /// Gets the unique integer hash of the id.
        /// </summary>
        /// <remarks>
        /// Hashing is done through the Hashing.FNV1Hash() static function.
        /// </remarks>
        /// <returns> A unique integer hash of the id. </returns>
        uint GetHash();

        /// <summary>
        /// Calculates the current color returned by the hair type.
        /// </summary>
        /// <param name="phase"> 
        /// A parameter between 0-1.
        /// representing the position along the length of the hair (accounting for speed).
        /// </param>
        /// <returns> The color of the section of the hair. </returns>
        Color GetColor(float phase);

        /// <summary>
        /// Reads the hair type from a stream.
        /// </summary>
        /// <param name="reader"> The stream to read from. </param>
        /// <param name="version"> The version array (size 3), form {Major,Minor,Sub} </param>
        void Read(BinaryReader reader, byte[] version);

        /// <summary>
        /// Writes the hair type to a stream.
        /// </summary>
        /// <param name="writer"> The stream to write to. </param>
        void Write(BinaryWriter writer);

        /// <summary>
        /// Creates a new instance of this hair type.
        /// </summary>
        /// <returns> An instance of the hair type. </returns>
        IHairType CreateNew();

        /// <summary>
        /// Creates a new instance of this hair type, depending on dash count.
        /// </summary>
        /// <remarks>
        /// Really only meant to be able to include default colors in a hair type.
        /// </remarks>
        /// <param name="i"> The dash count.</param>
        /// <returns> An instance of the hair type. </returns>
        IHairType CreateNew(int i);

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
        IHairType CreateNew(string str);

        /// <summary>
        /// Constructs a menu for the options of the hair type.
        /// </summary>
        /// <remarks>
        /// Don't add string editors if inGame is true, it'll cause everest to crash.
        /// </remarks>
        /// <param name="menu"> Top menu. </param>
        /// <param name="inGame"> Whether or not this is the in game options menu. </param>
        /// <returns> A list of menu option items to be added. </returns>
        List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame);

    }
}
