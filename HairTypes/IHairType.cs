using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace Celeste.Mod.Hyperline
{
    public interface IHairType
    {
        string GetHairName();
        Color GetColor(float phase);
        void Read(BinaryReader reader, byte[] version);
        void Write(BinaryWriter writer);
        IHairType CreateNew();
        IHairType CreateNew(int i);
        List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame);
    }
}
