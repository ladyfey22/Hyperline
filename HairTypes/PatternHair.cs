using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace Celeste.Mod.Hyperline
{
    public class PatternHair : IHairType
    {
        public const int MAX_PATTERN_COUNT = 10;

        public PatternHair()
        {
            colorList = new HSVColor[MAX_PATTERN_COUNT];
            for (int i = 0; i < colorList.Length; i++)
                colorList[i] = new HSVColor();
            patternCount = 0;
        }

        public static string NumToString(int i)
        {
            return i.ToString();
        }

        public string GetHairName()
        {
            return "MODOPTIONS_HYPERLINE_PATTERN";
        }

        public Color GetColor(float phase)
        {
            if (patternCount == 0)
                return Color.White;
            int index = (int)(patternCount * phase);
            index = Math.Min(index, patternCount - 1);
            return colorList[index].ToColor();
        }
        public void Read(BinaryReader reader, byte[] version)
        {
            patternCount = reader.ReadInt32();
            for (int i = 0; i < MAX_PATTERN_COUNT; i++)
            {
                colorList[i] = new HSVColor();
                colorList[i].Read(reader);
            }
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(patternCount);
            for (int i = 0; i < MAX_PATTERN_COUNT; i++)
                colorList[i].Write(writer);
        }

        public IHairType CreateNew()
        {
            return new PatternHair();
        }

        public void UpdatePatternCount(int v)
        {
            patternCount = v;
        }


        public List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame)
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

        public IHairType CreateNew(int i)
        {
            return new PatternHair();
        }

        public string GetId()
        {
            return id;
        }

        public uint GetHash()
        {
            return hash;
        }

        public static string id = "Hyperline_PatternHair";
        public static uint hash = Hashing.FNV1Hash(id);

        private HSVColor[] colorList;
        private int patternCount;
    }
}
