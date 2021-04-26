using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace Celeste.Mod.Hyperline
{
    public class RainbowHair : IHairType
    {
        private int saturation;
        private int value;

        public RainbowHair()
        {
            saturation = 10;
            value = 10;
        }

        public string GetHairName()
        {
            return "MODOPTIONS_HYPERLINE_RAINBOW";
        }
        public Color GetColor(float phase)
        {
            HSVColor returnV = new HSVColor(359 * phase, value / 10.0f, saturation / 10.0f);
            return returnV.ToColor();
        }

        public void Read(BinaryReader reader, byte[] version)
        {
            saturation = reader.ReadInt32();
            value = reader.ReadInt32();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(saturation);
            writer.Write(value);
        }

        private static string NumToString(int num)
        {
            return num.ToString();
        }

        public void UpdateSaturation(int v)
        {
            saturation = v;
        }

        public void UpdateValue(int v)
        {
            value = v;
        }

        public List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame)
        {
            List<TextMenu.Item> colorMenus = new List<TextMenu.Item>();
            colorMenus.Add(new TextMenu.Slider("Saturation:", NumToString, 0, 10, saturation).Change(UpdateSaturation));
            colorMenus.Add(new TextMenu.Slider("Value:", NumToString, 0, 10, value).Change(UpdateValue));
            return colorMenus;
        }
        public IHairType CreateNew()
        {
            return new GradientHair();
        }

        public IHairType CreateNew(int i)
        {
            return new RainbowHair();
        }

        public IHairType CreateNew(string str)
        {
            RainbowHair returnV = new RainbowHair();
            string[] tokens = str.Split(',');
            if (tokens.Length >= 1)
                returnV.saturation = int.Parse(tokens[0]);
            if (tokens.Length >= 2)
                returnV.value = int.Parse(tokens[1]);
            return returnV;
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
