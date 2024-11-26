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
            float min = Math.Min(Math.Min(r, g), b);
            float max = Math.Max(Math.Max(r, g), b);
            V = max;
            float delta = max - min;
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
                internalColor = new(v, t, p, 255);
            }
            else if (hi == 1)
            {
                internalColor = new(q, v, p, 255);
            }
            else if (hi == 2)
            {
                internalColor = new(p, v, t, 255);
            }
            else if (hi == 3)
            {
                internalColor = new(p, q, v, 255);
            }
            else if (hi == 4)
            {
                internalColor = new(t, p, v, 255);
            }
            else
            {
                internalColor = new(v, p, q, 255);
            }
        }


        public Color ToColor() => internalColor;

        /// <summary>
        /// Converts a string to a color. The string can be either an RGB or an HSV value.
        /// Format is RRGGBB or HHHSSSVVV.
        /// </summary>
        /// <param name="colorString"></param>
        /// <returns></returns>
        public bool FromString(string colorString, bool supress = false)
        {

            try
            {
                switch (colorString.Length)
                {
                    //Assumed to be an RGB value.
                    case 6:
                        FromColor(new(
                            int.Parse(colorString[..2], NumberStyles.HexNumber),
                            int.Parse(colorString.Substring(2, 2), NumberStyles.HexNumber),
                            int.Parse(colorString.Substring(4, 2), NumberStyles.HexNumber),
                            255));
                        break;
                    //Assumed to be an HSV value.
                    case 9:
                        H = int.Parse(colorString[..3], NumberStyles.Integer);
                        S = int.Parse(colorString.Substring(3, 3), NumberStyles.Integer) / 100.0f;
                        V = int.Parse(colorString.Substring(6, 3), NumberStyles.Integer) / 100.0f;
                        break;
                    default:
                        if (!supress)
                        {
                            Logger.Log(LogLevel.Warn, "Hyperline", "Invalid color string " + colorString);
                        }

                        return false;
                }
            }
            catch
            {
                if (!supress)
                {
                    Logger.Log(LogLevel.Warn, "Hyperline", "Invalid color string " + colorString);
                }
                return false;
            }
            UpdateColor();
            return true;
        }

        public static HSVColor operator -(HSVColor l, HSVColor r) => new(l.H - r.H, l.S - r.S, l.V - r.V);

        public static HSVColor operator +(HSVColor l, HSVColor r) => new(l.H + r.H, l.S + r.S, l.V = r.V);

        public static HSVColor operator *(HSVColor l, float r) => new(l.H * r, l.S * r, l.V * r);

        public override string ToString() => internalColor.R.ToString("X2") + internalColor.G.ToString("X2") + internalColor.B.ToString("X2");

        public string ToHSVString() => H.ToString("000") + (S * 100).ToString("000") + (V * 100).ToString("000");
    }
}
