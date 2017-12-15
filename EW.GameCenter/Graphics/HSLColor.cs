using System;
using EW.Scripting;
using EW.OpenGLES;
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

            //H = (byte)((color.GetHue() / 360.0f) * 255);
            //S = (byte)(color.GetSaturation() * 255);
            //L = (byte)(color.GetBrightness() * 255);

        }

        public HSLColor(byte h,byte s,byte l)
        {
            H = h;
            S = s;
            L = l;

        }


        public static bool TryParseRGB(string value,out Color color)
        {
            color = new Color();
            return true;
        }



        public override int GetHashCode()
        {
            return H.GetHashCode() ^ S.GetHashCode() ^ L.GetHashCode();
        }

        public static bool operator ==(HSLColor me,HSLColor other)
        {
            return me.H == other.H && me.S == other.S && me.L == other.L;
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



    }
}