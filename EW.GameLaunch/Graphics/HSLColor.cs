using System;
using System.Globalization;
using EW.Scripting;
using EW.OpenGLES;
using System.Drawing;
namespace EW.Graphics
{
    /// <summary>
    /// 色彩饱和度(hue,saturation,lightes)
    /// </summary>
    public class HSLColor:IScriptBindable
    {
        public readonly byte H;
        public readonly byte S;
        public readonly byte L;

        public readonly Color RGB;

        public HSLColor(Color color)
        {
            RGB = color;

            H = (byte)((color.GetHue() / 360.0f) * 255);
            S = (byte)(color.GetSaturation() * 255);
            L = (byte)(color.GetBrightness() * 255);

        }

        public HSLColor(byte h,byte s,byte l)
        {
            H = h;
            S = s;
            L = l;
            RGB = RGBFromHSL(H / 255f, S / 255f, L / 255f);

        }

        public static HSLColor FromRGB(int r, int g, int b)
        {
            return new HSLColor(Color.FromArgb(r, g, b));
        }


        public static bool TryParseRGB(string value, out Color color)
        {
            color = new Color();
            value = value.Trim();
            if (value.Length != 6 && value.Length != 8)
                return false;

            byte red, green, blue, alpha = 255;
            if (!byte.TryParse(value.Substring(0, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out red)
                || !byte.TryParse(value.Substring(2, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out green)
                || !byte.TryParse(value.Substring(4, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out blue))
                return false;

            if (value.Length == 8
                && !byte.TryParse(value.Substring(6, 2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out alpha))
                return false;

            color = Color.FromArgb(alpha, red, green, blue);
            return true;
        }



        public override int GetHashCode()
        {
            return H.GetHashCode() ^ S.GetHashCode() ^ L.GetHashCode();
        }

        public static bool operator ==(HSLColor me,HSLColor other)
        {
            return me.H == other.H && me.S == other.S && me.L == other.L || me.RGB == other.RGB;
        }

        public static bool operator !=(HSLColor me,HSLColor other)
        {
            return !(me == other);
        }

        public override bool Equals(object obj)
        {
            var o = obj as HSLColor;
            return o != null && o == this;
        }


        public static Color RGBFromHSL(float h, float s, float l)
        {
            // Convert from HSL to RGB
            var q = (l < 0.5f) ? l * (1 + s) : l + s - (l * s);
            var p = 2 * l - q;

            float[] trgb = { h + 1 / 3.0f, h, h - 1 / 3.0f };
            float[] rgb = { 0, 0, 0 };

            for (var k = 0; k < 3; k++)
            {
                while (trgb[k] < 0) trgb[k] += 1.0f;
                while (trgb[k] > 1) trgb[k] -= 1.0f;
            }

            for (var k = 0; k < 3; k++)
            {
                if (trgb[k] < 1 / 6.0f)
                    rgb[k] = p + ((q - p) * 6 * trgb[k]);
                else if (trgb[k] >= 1 / 6.0f && trgb[k] < 0.5)
                    rgb[k] = q;
                else if (trgb[k] >= 0.5f && trgb[k] < 2.0f / 3)
                    rgb[k] = p + ((q - p) * 6 * (2.0f / 3 - trgb[k]));
                else
                    rgb[k] = p;
            }

            return Color.FromArgb((int)(rgb[0] * 255), (int)(rgb[1] * 255), (int)(rgb[2] * 255));
        }

        public override string ToString()
        {
            return "{0},{1},{2}".F(H, S, L);
        }


        public static string ToHexString(Color color)
        {
            if (color.A == 255)
                return color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
            return color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2") + color.A.ToString("X2");
        }


    }
}