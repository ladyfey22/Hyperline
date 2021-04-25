using Celeste.Mod.UI;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;

namespace Celeste.Mod.Hyperline
{
    public class GradientHair : IHairType
    {
        public GradientHair()
        {
            color1 = new HSVColor();
            color2 = new HSVColor();
        }

        public string GetHairName()
        {
            return "MODOPTIONS_HYPERLINE_GRADIENT";
        }
        public Color GetColor(float phase)
        {
            phase = (float)Math.Sin(2 * Math.PI * (phase)) / 2.0f + 0.5f;
            if (doRgbGradient)
                return new Color((byte)(color1.R + ((float)color2.R - color1.R) * phase),
                                 (byte)(color1.G + ((float)color2.G - color1.G) * phase),
                                 (byte)(color1.B + ((float)color2.B - color1.B) * phase));
            return new HSVColor(color1.H + (color2.H - color1.H) * phase,
               color1.S + (color2.S - color1.S) * phase,
               color1.V + (color2.V - color1.V) * phase).ToColor();
        }

        public void Read(BinaryReader reader, byte[] version)
        {
            color1.Read(reader);
            color2.Read(reader);
            if (version[0] >= 0 && version[1] >= 1 && version[2] >= 8)
                doRgbGradient = reader.ReadBoolean();
        }
        public void Write(BinaryWriter writer)
        {
            color1.Write(writer);
            color2.Write(writer);
            writer.Write(doRgbGradient);
        }

        public IHairType CreateNew()
        {
            return new GradientHair();
        }

        public List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame)
        {
            List<TextMenu.Item> colorMenus = new List<TextMenu.Item>();
            colorMenus.Add(new TextMenu.Button("Color 1: " + color1.ToString()).Pressed(() =>
            {
                Audio.Play(SFX.ui_main_savefile_rename_start);
                menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(color1.ToString(), v => { color1 = new HSVColor(v); }, 9);
            }));

            colorMenus.Add(new TextMenu.Button("Color 2: " + color2.ToString()).Pressed(() =>
            {
                Audio.Play(SFX.ui_main_savefile_rename_start);
                menu.SceneAs<Overworld>().Goto<OuiModOptionString>().Init<OuiModOptions>(color2.ToString(), v => { color2 = new HSVColor(v); }, 9);
            }));

            colorMenus.Add(new TextMenu.OnOff("Rgb Gradient", doRgbGradient).Change(v => doRgbGradient = v));

            return colorMenus;
        }

        public IHairType CreateNew(int i)
        {
            return new GradientHair();
        }

        public string GetId()
        {
            return id;
        }

        public uint GetHash()
        {
            return hash;
        }


        public static string id = "Hyperline_GradientHair";
        public static uint hash = Hashing.FNV1Hash(id);

        private HSVColor color1;
        private HSVColor color2;
        private bool doRgbGradient = false;
    }
}

