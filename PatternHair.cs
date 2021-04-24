using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using System.IO;
using Celeste.Mod.UI;

namespace Celeste.Mod.Hyperline
{
    public class PatternHair : IHairType
    {
        const int MAX_PATTERN_COUNT=10;
        
        public PatternHair()
        {
            ColorList = new HSVColor[MAX_PATTERN_COUNT];
            for (int i = 0; i < ColorList.Length; i++)
                ColorList[i] = new HSVColor();
            PatternCount = 0;
        }
        
        static string NumToString(int i)
        {
            return i.ToString();
        }
        public override string GetHairName()
        {
            return "MODOPTIONS_HYPERLINE_PATTERN";
        }
        public override Color GetColor(float phase)
        {
            if (PatternCount == 0)
                return Color.White;
            int index = (int)(PatternCount * phase);
            index = Math.Min(index, PatternCount-1);
            return ColorList[index].ToColor();
        }
        public override void Read(BinaryReader reader, byte[] version)
        {
            PatternCount = reader.ReadInt32();
            for (int i = 0; i < MAX_PATTERN_COUNT; i++)
            {
                ColorList[i] = new HSVColor();
                ColorList[i].Read(reader);
            }
        }
        public override void Write(BinaryWriter writer)
        {
            writer.Write(PatternCount);
            for (int i = 0; i < MAX_PATTERN_COUNT; i++)
                ColorList[i].Write(writer);
        }

        public override IHairType CreateNew()
        {
            return new PatternHair();
        }

        public void UpdatePatternCount(int v)
        {

            PatternCount = v;
        }


        public override List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame)
        {
            List<TextMenu.Item> colorMenus = new List<TextMenu.Item>();
            colorMenus.Add(new TextMenu.Slider("Pattern Count: ", NumToString, 1, MAX_PATTERN_COUNT, PatternCount).Change(UpdatePatternCount));
            for (int i = 0; i < MAX_PATTERN_COUNT; i++)
            {
                int counter = i;
                colorMenus.Add(new TextMenu.Button("Color " + (counter + 1) + ": " + ColorList[counter].ToString()).Pressed(() =>
                  {
                      Audio.Play(SFX.ui_main_savefile_rename_start);
                      menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(ColorList[counter].ToString(), v => { ColorList[counter] = new HSVColor(v); }, 9);
                  }));
            }
            return colorMenus;
        }

        public override IHairType CreateNew(int i)
        {
            return new PatternHair();
        }

        HSVColor[] ColorList;
        int PatternCount;
    }
}
