namespace Celeste.Mod.Hyperline
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;

    public class RainbowHair : IHairType
    {
        private int saturation;
        private int value;

        private const int MinSaturation = 0;
        private const int MaxSaturation = 10;

        private const int MinValue = 0;
        private const int MaxValue = 10;

        public RainbowHair()
        {
            saturation = MaxSaturation;
            value = MaxValue;
        }

        public RainbowHair(RainbowHair rvalue)
        {
            saturation = rvalue.saturation;
            value = rvalue.value;
        }

        public override IHairType Clone() => new RainbowHair(this);

        public override string GetHairName() => "MODOPTIONS_HYPERLINE_RAINBOW";
        public override Color GetColor(Color colorOrig, float phase)
        {
            HSVColor returnV = new(359 * phase, value / MaxValue, saturation / MaxSaturation);
            return returnV.ToColor();
        }

        public static string NumToString(int num) => num.ToString();

        public void UpdateSaturation(int v) => saturation = v;

        public void UpdateValue(int v) => value = v;

        public override List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame)
        {
            List<TextMenu.Item> colorMenus =
            [
                new TextMenu.Slider("Saturation:", NumToString, 0, 10, saturation).Change(UpdateSaturation),
                new TextMenu.Slider("Value:", NumToString, 0, 10, value).Change(UpdateValue),
            ];
            return colorMenus;
        }
        public override IHairType CreateNew() => new RainbowHair();
        public override IHairType CreateNew(int i) => new RainbowHair();

        public override IHairType CreateNew(string str)
        {
            RainbowHair returnV = new();
            string[] tokens = str.Split(',');
            if (tokens.Length >= 1)
            {
                returnV.saturation = int.Parse(tokens[0]);
            }

            if (tokens.Length >= 2)
            {
                returnV.value = int.Parse(tokens[1]);
            }

            return returnV;
        }

        public override string GetId() => Id;
        public override uint GetHash() => Hash;
        public override void Read(BinaryReader reader, byte[] version) { }
        public override void Write(BinaryWriter writer) { }

        public override void Read(XElement element)
        {
            saturation = Math.Clamp((int?)element.Element("saturation") ?? saturation, MinSaturation, MaxSaturation);
            value = Math.Clamp((int?)element.Element("value") ?? value, MinValue, MaxValue);
        }

        public override void Write(XElement element) => element.Add(new XElement("saturation", saturation), new XElement("value", value));

        public const string Id = "Hyperline_RainbowHair";
        public static readonly uint Hash = Hashing.FNV1Hash(Id);
    }
}
