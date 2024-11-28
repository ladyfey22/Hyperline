namespace Celeste.Mod.Hyperline.HairTypes
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Xml.Linq;
    using Lib.Utility;
    using Lib;
    using Microsoft.Xna.Framework;
    using Lib.UI;

    public class GradientHair : BaseHairType
    {
        private HSVColor color1; // the start color of the gradient
        private HSVColor color2; // the end color of the gradient
        private bool doRgbGradient; // whether to go along the HSV spectrum, or to just directly linearly interpolate the rgb values
        private int cycles; // the number of cycles of the gradient that run along the hair length
        private bool colorReturn; // whether or not the color should return back to the start color before the end of the cycle

        private const int MinCycles = 1;
        private const int MaxCycles = 10;

        public GradientHair()
        {
            color1 = new();
            color2 = new();
            doRgbGradient = false;
            colorReturn = false;
            cycles = 1;
            colorReturn = true;
        }

        public GradientHair(GradientHair rvalue)
        {
            color1 = rvalue.color1.Clone();
            color2 = rvalue.color2.Clone();
            doRgbGradient = rvalue.doRgbGradient;
            cycles = rvalue.cycles;
            colorReturn = rvalue.colorReturn;
        }

        public override IHairType Clone() => new GradientHair(this);
        public override string GetHairName() => "MODOPTIONS_HYPERLINE_GRADIENT";

        public override Color GetColor(Color colorOrig, float phase)
        {
            //phase = ((float)Math.Sin(2 * Math.PI * phase) / 2.0f) + 0.5f;

            // make the number of cycles desired, and get only the decimal part to make it zero to one
            phase = (phase * cycles) - (float)Math.Truncate(phase * cycles);

            // now we need to make a triangle gradient, so zero to one back to zero within the phase [0-1]
            if (colorReturn)
            {
                phase = 1 - Math.Abs(1 - (2 * phase));
            }

            if (doRgbGradient)
            {
                return new((byte)(color1.R + (((float)color2.R - color1.R) * phase)),
                                 (byte)(color1.G + (((float)color2.G - color1.G) * phase)),
                                 (byte)(color1.B + (((float)color2.B - color1.B) * phase)));
            }

            return new HSVColor(color1.H + ((color2.H - color1.H) * phase),
               color1.S + ((color2.S - color1.S) * phase),
               color1.V + ((color2.V - color1.V) * phase)).ToColor();
        }


        public override List<TextMenu.Item> CreateMenu(TextMenu menu, bool inGame)
        {
            List<TextMenu.Item> colorMenus = [];

            colorMenus.Add(new ColorSubmenu(menu, "Color 1 ", color1, inGame).Change(v => color1 = new(v)));
            colorMenus.Add(new ColorSubmenu(menu, "Color 2 ", color2, inGame).Change(v => color2 = new(v)));
            colorMenus.Add(new TextMenu.OnOff("Rgb Gradient", doRgbGradient).Change(v => doRgbGradient = v));
            colorMenus.Add(new TextMenu.Slider("Cycles", v => v.ToString(), MinCycles, MaxCycles, cycles).Change(v => cycles = v));
            colorMenus.Add(new TextMenu.OnOff("Do Return Gradient", colorReturn).Change(v => colorReturn = v));

            return colorMenus;
        }

        public override IHairType CreateNew() => new GradientHair();
        public override IHairType CreateNew(int i) => new GradientHair();

        public override IHairType CreateNew(string str)
        {
            GradientHair returnV = new();
            string[] tokenList = str.Split(',');
            if (tokenList.Length < 2)
            {
                return returnV;
            }

            returnV.color1 = new(tokenList[0]);
            returnV.color2 = new(tokenList[1]);
            if (tokenList.Length >= 3)
            {
                returnV.doRgbGradient = bool.Parse(tokenList[2]);
            }

            return returnV;
        }

        public override string GetId() => Id;
        public override uint GetHash() => Hash;
        public override void Read(BinaryReader reader, byte[] version) { }
        public override void Write(BinaryWriter writer) { }

        public override void Read(XElement element)
        {
            ReadColorElement(element, "color1", ref color1);
            ReadColorElement(element, "color2", ref color2);
            cycles = Math.Clamp((int?)element.Element("cycles") ?? cycles, MinCycles, MaxCycles);
            doRgbGradient = (bool?)element.Element("doRgbGradient") ?? doRgbGradient;
            colorReturn = (bool?)element.Element("colorReturn") ?? colorReturn;
        }

        public override void Write(XElement element) => element.Add(new XElement("color1", color1.ToHSVString()), new XElement("color2", color2.ToHSVString()), new XElement("doRgbGradient", doRgbGradient), new XElement("cycles", cycles), new XElement("colorReturn", colorReturn));

        public const string Id = "Hyperline_GradientHair";
        public static readonly uint Hash = Hashing.FNV1Hash(Id);
    }
}

