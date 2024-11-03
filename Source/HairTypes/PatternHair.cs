namespace Celeste.Mod.Hyperline
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;

    public class PatternHair : IHairType
    {
        public const int MaxPatternCount = 10;
        private readonly HSVColor[] colorList;
        private int patternCount;

        public PatternHair()
        {
            colorList = new HSVColor[MaxPatternCount];
            for (int i = 0; i < colorList.Length; i++)
            {
                colorList[i] = new HSVColor();
            }

            patternCount = 0;
        }

        public PatternHair(PatternHair rvalue)
        {
            colorList = new HSVColor[MaxPatternCount];
            for (int i = 0; i < colorList.Length; i++)
            {
                colorList[i] = rvalue.colorList[i].Clone();
            }

            patternCount = rvalue.patternCount;
        }

        public override IHairType Clone() => new PatternHair(this);

        public static string NumToString(int i) => i.ToString();

        public override string GetHairName() => "MODOPTIONS_HYPERLINE_PATTERN";

        public override Color GetColor(Color colorOrig, float phase)
        {
            if (patternCount == 0)
            {
                return Color.White;
            }

            int index = (int)(patternCount * phase);
            index = Math.Min(index, patternCount - 1);
            return colorList[index].ToColor();
        }

        public void UpdatePatternCount(int v) => patternCount = v;

        public override List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame)
        {
            List<TextMenu.Item> colorMenus =
            [
                new TextMenu.Slider("Pattern Count: ", NumToString, 1, MaxPatternCount, patternCount).Change(UpdatePatternCount),
            ];

            for (int i = 0; i < MaxPatternCount; i++)
            {
                int counter = i;
                colorMenus.Add(new UI.ColorSubmenu(menu, "Color " + (i + 1), colorList[counter], inGame).Change(v => colorList[counter] = new HSVColor(v)));

            }
            return colorMenus;
        }

        public override IHairType CreateNew() => new PatternHair();

        public override IHairType CreateNew(int i) => new PatternHair();

        public override IHairType CreateNew(string str)
        {
            PatternHair returnV = new();
            string[] tokens = str.Split(',');

            if (tokens.Length < 1) //no length paramter
            {
                return returnV;
            }

            returnV.patternCount = int.Parse(tokens[0]);
            for (int i = 0; i < returnV.patternCount && i < returnV.colorList.Length && i + 1 < tokens.Length; i++)
            {
                returnV.colorList[i] = new HSVColor(tokens[i + 1]);
            }

            return returnV;
        }

        public override string GetId() => Id;
        public override uint GetHash() => Hash;
        public override void Read(BinaryReader reader, byte[] version) { }
        public override void Write(BinaryWriter writer) { }
        public override void Read(XElement element)
        {
            patternCount = Math.Clamp((int?)element.Element("patternCount") ?? patternCount, 1, MaxPatternCount);

            int index = 0;
            foreach (XElement currentElement in element.Elements("color"))
            {
                if (index < colorList.Length)
                {
                    colorList[index].FromString((string)currentElement);
                    index++;
                }
            }
        }

        public override void Write(XElement element)
        {
            XElement[] elements = new XElement[colorList.Length + 1];
            elements[0] = new XElement("patternCount", patternCount);
            for (int i = 0; i < colorList.Length; i++)
            {
                elements[i + 1] = new XElement("color", colorList[i].ToHSVString());
            }

            element.Add(elements);
        }

        public const string Id = "Hyperline_PatternHair";
        public static readonly uint Hash = Hashing.FNV1Hash(Id);
    }
}
