using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace Celeste.Mod.Hyperline
{
    public class PatternHair : IHairType
    {
        public const int MAX_PATTERN_COUNT = 10;
        private HSVColor[] colorList;
        private int patternCount;

        public PatternHair()
        {
            colorList = new HSVColor[MAX_PATTERN_COUNT];
            for (int i = 0; i < colorList.Length; i++)
                colorList[i] = new HSVColor();
            patternCount = 0;
        }

        public PatternHair(PatternHair rvalue)
        {
            colorList = new HSVColor[MAX_PATTERN_COUNT];
            for (int i = 0; i < colorList.Length; i++)
                colorList[i] = rvalue.colorList[i].Clone();
            patternCount = rvalue.patternCount;
        }

        public override IHairType Clone()
        {
            return new PatternHair(this);
        }

        public static string NumToString(int i)
        {
            return i.ToString();
        }

        public override string GetHairName()
        {
            return "MODOPTIONS_HYPERLINE_PATTERN";
        }

        public override Color GetColor(Color colorOrig, float phase)
        {
            if (patternCount == 0)
                return Color.White;
            int index = (int)(patternCount * phase);
            index = Math.Min(index, patternCount - 1);
            return colorList[index].ToColor();
        }
        public override void Read(BinaryReader reader, byte[] version)
        {
            patternCount = reader.ReadInt32();
            for (int i = 0; i < MAX_PATTERN_COUNT; i++)
            {
                colorList[i] = new HSVColor();
                colorList[i].Read(reader);
            }
        }
        public override void Write(BinaryWriter writer)
        {
            writer.Write(patternCount);
            for (int i = 0; i < MAX_PATTERN_COUNT; i++)
                colorList[i].Write(writer);
        }

        public void UpdatePatternCount(int v)
        {
            patternCount = v;
        }

        public override List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame)
        {
            List<TextMenu.Item> colorMenus = new List<TextMenu.Item>();
            colorMenus.Add(new TextMenu.Slider("Pattern Count: ", NumToString, 1, MAX_PATTERN_COUNT, patternCount).Change(UpdatePatternCount));
            for (int i = 0; i < MAX_PATTERN_COUNT; i++)
            {
                int counter = i;
                colorMenus.Add(new TextMenu.Button("Color " + (counter + 1) + ": " + colorList[counter].ToString()).Pressed(() =>
                  {
                      Audio.Play(SFX.ui_main_savefile_rename_start);
                      menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(colorList[counter].ToString(), v => { colorList[counter] = new HSVColor(v); }, 9);
                  }));
            }
            return colorMenus;
        }

        public override IHairType CreateNew()
        {
            return new PatternHair();
        }

        public override IHairType CreateNew(int i)
        {
            return new PatternHair();
        }

        public override IHairType CreateNew(string str)
        {
            PatternHair returnV = new PatternHair();
            string[] tokens = str.Split(',');

            if (tokens.Length < 1) //no length paramter
                return returnV;
            returnV.patternCount = int.Parse(tokens[0]);
            for (int i = 0; i < returnV.patternCount && i < returnV.colorList.Length && i + 1 < tokens.Length; i++)
                returnV.colorList[i] = new HSVColor(tokens[i + 1]);
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
            XElement patternCountElement = element.Element("patternCount");
            if (patternCountElement != null)
                patternCount = (int)patternCountElement;
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
                elements[i + 1] = new XElement("color", colorList[i].ToHSVString());
            element.Add(elements);
        }

        public static string id = "Hyperline_PatternHair";
        public static uint hash = Hashing.FNV1Hash(id);
    }
}
