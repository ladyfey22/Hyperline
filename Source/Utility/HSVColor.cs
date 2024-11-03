namespace Celeste.Mod.Hyperline
{
    using Microsoft.Xna.Framework;
    using System;
    using System.Globalization;

    /// <summary>
    /// A class representing color as Hue, Saturation, and Value.
    /// </summary>
    public class HSVColor
    {

        public float H { get; protected set; }
        public float S { get; protected set; }
        public float V { get; protected set; }

        public byte R => internalColor.R;

        public byte G => internalColor.G;

        public byte B => internalColor.B;

        private Color internalColor;

        public HSVColor(float h = 0.0f, float s = .0f, float v = 0.0f)
        {
            H = h;
            S = s;
            V = v;
            UpdateColor();
        }

        public HSVColor(HSVColor rvalue)
        {
            H = rvalue.H;
            S = rvalue.S;
            V = rvalue.V;
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

        public HSVColor Clone() => new(this);

        public void FromColor(Color colorIn)
        {
            float r = colorIn.R / 255f;
            float g = colorIn.G / 255f;
            float b = colorIn.B / 255f;
            float min, max, delta;
            min = Math.Min(Math.Min(r, g), b);
            max = Math.Max(Math.Max(r, g), b);
            V = max;
            delta = max - min;
            if (max != 0)
            {
                S = delta / max;
                if (r == max)
                {
                    H = (g - b) / delta;
                }
                else if (g == max)
                {
                    H = 2 + ((b - r) / delta);
                }
                else
                {
                    H = 4 + ((r - g) / delta);
                }

                H *= 60f;
                if (H < 0)
                {
                    H += 360f;
                }
            }
            else
            {
                S = 0f;
                H = 0f;
            }
            if (float.IsNaN(H))
            {
                H = 0.0f;
            }

            if (float.IsNaN(S))
            {
                S = 0.0f;
            }

            if (float.IsNaN(V))
            {
                V = 0.0f;
            }

            UpdateColor();
        }

        private void UpdateColor()
        {
            int hi = (int)Math.Floor(H / 60f) % 6;
            float f = (H / 60f) - (float)Math.Floor(H / 60f);
            float vf = V * 255;
            int v = (int)Math.Round(vf);
            int p = (int)Math.Round(vf * (1 - S));
            int q = (int)Math.Round(vf * (1 - (f * S)));
            int t = (int)Math.Round(vf * (1 - ((1 - f) * S)));
            if (hi == 0)
            {
                internalColor = new Color(v, t, p, 255);
            }
            else if (hi == 1)
            {
                internalColor = new Color(q, v, p, 255);
            }
            else if (hi == 2)
            {
                internalColor = new Color(p, v, t, 255);
            }
            else if (hi == 3)
            {
                internalColor = new Color(p, q, v, 255);
            }
            else if (hi == 4)
            {
                internalColor = new Color(t, p, v, 255);
            }
            else
            {
                internalColor = new Color(v, p, q, 255);
            }
        }


        public Color ToColor() => internalColor;

        public void FromString(string colorString)
        {

            try
            {
                if (colorString.Length == 6)    //Assumed to be an RGB value.
                {
                    FromColor(new Color(
                                int.Parse(colorString[..2], NumberStyles.HexNumber),
                                int.Parse(colorString.Substring(2, 2), NumberStyles.HexNumber),
                                int.Parse(colorString.Substring(4, 2), NumberStyles.HexNumber),
                                255));
                }
                else
                if (colorString.Length == 9) //Assumed to be an HSV value.
                {
                    H = int.Parse(colorString[..3], NumberStyles.Integer);
                    S = int.Parse(colorString.Substring(3, 3), NumberStyles.Integer) / 100.0f;
                    V = int.Parse(colorString.Substring(6, 3), NumberStyles.Integer) / 100.0f;
                }
                else
                {
                    FromColor(Color.White);
                }
            }
            catch
            {
                Logger.Log(LogLevel.Warn, "Hyperline", "Error reading a color value " + colorString);
            }
            UpdateColor();
        }

        public static HSVColor operator -(HSVColor l, HSVColor r) => new(l.H - r.H, l.S - r.S, l.V - r.V);

        public static HSVColor operator +(HSVColor l, HSVColor r) => new(l.H + r.H, l.S + r.S, l.V = r.V);

        public static HSVColor operator *(HSVColor l, float r) => new(l.H * r, l.S * r, l.V * r);

        public override string ToString() => internalColor.R.ToString("X2") + internalColor.G.ToString("X2") + internalColor.B.ToString("X2");

        public string ToHSVString() => H.ToString("000") + (S * 100).ToString("000") + (V * 100).ToString("000");
    }
}
