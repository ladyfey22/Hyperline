using Microsoft.Xna.Framework;
using System;
using System.Globalization;
using System.IO;

namespace Celeste.Mod.Hyperline
{
    public class HSVColor
    {
        public HSVColor(float h = 0.0f, float s = .0f, float v = 0.0f)
        {
            H = h;
            S = s;
            V = v;
            UpdateColor();
        }

        public HSVColor(string code)
        {
            FromString(code);
        }

        public HSVColor(Color inColor)
        {
            FromColor(inColor);
        }

        public void FromColor(Color ColorIn)
        {
            float r = ColorIn.R / 255f;
            float g = ColorIn.G / 255f;
            float b = ColorIn.B / 255f;
            float min, max, delta;
            min = Math.Min(Math.Min(r, g), b);
            max = Math.Max(Math.Max(r, g), b);
            V = max;
            delta = max - min;
            if (max != 0)
            {
                S = delta / max;
                if (r == max)
                    H = (g - b) / delta;
                else if (g == max)
                    H = 2 + (b - r) / delta;
                else
                    H = 4 + (r - g) / delta;
                H *= 60f;
                if (H < 0)
                    H += 360f;
            }
            else
            {
                S = 0f;
                H = 0f;
            }
            if (float.IsNaN(H))
                H = 0.0f;
            if (float.IsNaN(S))
                S = 0.0f;
            if (float.IsNaN(V))
                V = 0.0f;
            UpdateColor();
        }

        private void UpdateColor()
        {
            int hi = (int)(Math.Floor(H / 60f)) % 6;
            float f = H / 60f - (float)Math.Floor(H / 60f);
            float Vf = V * 255;
            int v = (int)Math.Round(Vf);
            int p = (int)Math.Round(Vf * (1 - S));
            int q = (int)Math.Round(Vf * (1 - f * S));
            int t = (int)Math.Round(Vf * (1 - (1 - f) * S));
            if (hi == 0)
                InternalColor = new Color(v, t, p, 255);
            else if (hi == 1)
                InternalColor = new Color(q, v, p, 255);
            else if (hi == 2)
                InternalColor = new Color(p, v, t, 255);
            else if (hi == 3)
                InternalColor = new Color(p, q, v, 255);
            else if (hi == 4)
                InternalColor = new Color(t, p, v, 255);
            else
                InternalColor = new Color(v, p, q, 255);
        }


        public Color ToColor()
        {
            return InternalColor;
        }

        private void FromString(string ColorString)
        {

            try
            {
                if (ColorString.Length == 6)    //Assumed to be an RGB value
                    FromColor(new Color(
                                int.Parse(ColorString.Substring(0, 2), NumberStyles.HexNumber),
                                int.Parse(ColorString.Substring(2, 2), NumberStyles.HexNumber),
                                int.Parse(ColorString.Substring(4, 2), NumberStyles.HexNumber),
                                255));
                else
                if (ColorString.Length == 9)
                {
                    H = int.Parse(ColorString.Substring(0, 3), NumberStyles.Integer);
                    S = int.Parse(ColorString.Substring(3, 3), NumberStyles.Integer) / 100.0f;
                    V = int.Parse(ColorString.Substring(6, 3), NumberStyles.Integer) / 100.0f;
                }
                else
                    FromColor(Color.White);
            }
            catch
            {
                Logger.Log(LogLevel.Warn, "Hyperline", "Error reading a color value.\n");
            }
            UpdateColor();
        }

        public static HSVColor operator -(HSVColor l, HSVColor r)
        {
            return new HSVColor(l.H - r.H, l.S - r.S, l.V - r.V);
        }

        public static HSVColor operator +(HSVColor l, HSVColor r)
        {
            return new HSVColor(l.H + r.H, l.S + r.S, l.V = r.V);
        }

        public static HSVColor operator *(HSVColor l, float r)
        {
            return new HSVColor(l.H * r, l.S * r, l.V * r);
        }

        public override string ToString()
        {
            return InternalColor.R.ToString("X2") + InternalColor.G.ToString("X2") + InternalColor.B.ToString("X2");
        }

        public void Read(BinaryReader reader)
        {
            H = reader.ReadSingle();
            S = reader.ReadSingle();
            V = reader.ReadSingle();
            UpdateColor();
        }

        public void Write(BinaryWriter writer)
        {
            writer.Write(H);
            writer.Write(S);
            writer.Write(V);
        }

        public float H { get; protected set; }
        public float S { get; protected set; }
        public float V { get; protected set; }

        public byte R
        {
            get
            {
                return InternalColor.R;
            }
        }

        public byte G
        {
            get
            {
                return InternalColor.G;
            }
        }

        public byte B
        {
            get
            {
                return InternalColor.B;
            }
        }


        private Color InternalColor;
    }
}
