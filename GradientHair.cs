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
    public class GradientHair : IHairType
    {
        public GradientHair()
        {
            Color1 = new HSVColor();
            Color2 = new HSVColor();
        }

        public string GetHairName()
        {
            return "MODOPTIONS_HYPERLINE_GRADIENT";
        }
        public Color GetColor(float phase)
        {
            phase = (float)Math.Sin(2 * Math.PI * (phase)) / 2.0f + 0.5f;
            if (doRgbGradient)
                return new Color((byte)(Color1.R + ((float)Color2.R - Color1.R) * phase),
                                 (byte)(Color1.G + ((float)Color2.G - Color1.G) * phase),
                                 (byte)(Color1.B + ((float)Color2.B - Color1.B) * phase));
            return new HSVColor(Color1.H + (Color2.H - Color1.H) * phase,
               Color1.S + (Color2.S - Color1.S) * phase,
               Color1.V + (Color2.V - Color1.V) * phase).ToColor();
        }

        public void Read(BinaryReader reader, byte[] version)
        {
            Color1.Read(reader);
            Color2.Read(reader);
            if (version[0] >= 0 && version[1] >= 1 && version[2] >= 8)
                doRgbGradient=reader.ReadBoolean();
        }
        public void Write(BinaryWriter writer)
        {
            Color1.Write(writer);
            Color2.Write(writer);
            writer.Write(doRgbGradient);
        }

        public IHairType CreateNew()
        {
            return new GradientHair();
        }
        public List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame)
        {
            List<TextMenu.Item> colorMenus = new List<TextMenu.Item>();
            colorMenus.Add(new TextMenu.Button("Color 1: " + Color1.ToString()).Pressed(() =>
            {
                Audio.Play(SFX.ui_main_savefile_rename_start);
                menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(Color1.ToString(), v => { Color1 = new HSVColor(v);}, 9);
            }));

            colorMenus.Add(new TextMenu.Button("Color 2: " + Color2.ToString()).Pressed(() =>
            {
                Audio.Play(SFX.ui_main_savefile_rename_start);
                menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(Color2.ToString(), v => { Color2 = new HSVColor(v); }, 9);
            }));

            colorMenus.Add(new TextMenu.OnOff("Rgb Gradient", doRgbGradient).Change(v => doRgbGradient = v));

            return colorMenus;
        }

        public IHairType CreateNew(int i)
        {
            return new GradientHair();
        }

        HSVColor Color1;
        HSVColor Color2;
        bool doRgbGradient = false;
    }
}

