using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Celeste.Mod.Hyperline
{
    public class RainbowHair : IHairType
    {
        public RainbowHair()
        {
        }

        public string GetHairName()
        {
            return "MODOPTIONS_HYPERLINE_RAINBOW";
        }
        public Color GetColor(float phase)
        {
            HSVColor returnV = new HSVColor(359 * phase, 1.0f, 1.0f);
            return returnV.ToColor();
        }

        public void Read(BinaryReader reader, byte[] version)
        {
        }

        public void Write(BinaryWriter writer)
        {
        }

        public IHairType CreateNew()
        {
            return new GradientHair();
        }

        public List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame)
        {
            return new List<TextMenu.Item>();
        }

        public IHairType CreateNew(int i)
        {
            return new RainbowHair();
        }

        public string GetId()
        {
            return id;
        }

        public uint GetHash()
        {
            return hash;
        }


        public static string id = "Hyperline_RainbowHair";
        public static uint hash = Hashing.FNV1Hash(id);
    }
}
