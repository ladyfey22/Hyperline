using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

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

        public RainbowHair(RainbowHair rvalue)
        {
            saturation = rvalue.saturation;
            value = rvalue.value;
        }

        public override IHairType Clone()
        {
            return new RainbowHair(this);
        }

        public override string GetHairName()
        {
            return "MODOPTIONS_HYPERLINE_RAINBOW";
        }
        public override Color GetColor(Color colorOrig, float phase)
        {
            HSVColor returnV = new HSVColor(359 * phase, value / 10.0f, saturation / 10.0f);
            return returnV.ToColor();
        }

        public override void Read(BinaryReader reader, byte[] version)
        {
            saturation = reader.ReadInt32();
            value = reader.ReadInt32();
        }

        public override void Write(BinaryWriter writer)
        {
            writer.Write(saturation);
            writer.Write(value);
        }

        public static string NumToString(int num)
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

        public override List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame)
        {
            List<TextMenu.Item> colorMenus = new List<TextMenu.Item>();
            colorMenus.Add(new TextMenu.Slider("Saturation:", NumToString, 0, 10, saturation).Change(UpdateSaturation));
            colorMenus.Add(new TextMenu.Slider("Value:", NumToString, 0, 10, value).Change(UpdateValue));
            return colorMenus;
        }
        public override IHairType CreateNew()
        {
            return new RainbowHair();
        }

        public override IHairType CreateNew(int i)
        {
            return new RainbowHair();
        }

        public override IHairType CreateNew(string str)
        {
            RainbowHair returnV = new RainbowHair();
            string[] tokens = str.Split(',');
            if (tokens.Length >= 1)
                returnV.saturation = int.Parse(tokens[0]);
            if (tokens.Length >= 2)
                returnV.value = int.Parse(tokens[1]);
            return returnV;
        }

        public override string GetId()
        {
            return id;
        }

        public override uint GetHash()
        {
            return hash;
        }

        public override void Read(XElement element)
        {
            XElement satElement = element.Element("saturation");
            XElement valElement = element.Element("value");
            if (satElement != null)
                saturation = (int)satElement;
            if (valElement != null)
                value = (int)valElement;
        }

        public override void Write(XElement element)
        {
            element.Add(new XElement("saturation", saturation), new XElement("value", value));
        }

        public static string id = "Hyperline_RainbowHair";
        public static uint hash = Hashing.FNV1Hash(id);
    }
}
