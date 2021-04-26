using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.IO;

namespace Celeste.Mod.Hyperline
{
    public class SolidHair : IHairType
    {
        public SolidHair()
        {
            C = new HSVColor();
        }

        public SolidHair(HSVColor color)
        {
            C = color;
        }

        public string GetHairName()
        {
            return "MODOPTIONS_HYPERLINE_SOLID";
        }

        public Color GetColor(float phase)
        {
            return C.ToColor();
        }

        public void Read(BinaryReader reader, byte[] version)
        {
            C.Read(reader);
        }

        public void Write(BinaryWriter writer)
        {
            C.Write(writer);
        }

        public List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame)
        {
            List<TextMenu.Item> colorMenus = new List<TextMenu.Item>();
            colorMenus.Add(new TextMenu.Button("Color 1: " + C.ToString()).Pressed(() =>
            {
                Audio.Play(SFX.ui_main_savefile_rename_start);
                menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(C.ToString(), v => { C = new HSVColor(v); }, 9);
            }));
            return colorMenus;
        }
        public IHairType CreateNew()
        {
            return new SolidHair();
        }

        public IHairType CreateNew(int i)
        {
            return new SolidHair(defaultColors[i % defaultColors.Length]);
        }

        public IHairType CreateNew(string str)
        {
            SolidHair returnV = new SolidHair();
            string[] tokens = str.Split(',');
            if (tokens.Length >= 1)
                returnV.C = new HSVColor(tokens[0]);
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


        public static string id = "Hyperline_SolidHair";
        public static uint hash = Hashing.FNV1Hash(id);

        private static HSVColor[] defaultColors = { new HSVColor("44B7FF"), new HSVColor("AC3232"), new HSVColor("FF6DEF") };
        private HSVColor C;
    }
}
