using System;
using System.Runtime.Serialization;

namespace EW.Xna.Platforms
{
    /// <summary>
    /// 32-bit color
    /// </summary>
    public struct Color:IEquatable<Color>
    {

        private uint _packedValue;


        public UInt32 PackedValue
        {
            get { return _packedValue; }
            set { _packedValue = value; }
        }
        static Color()
        {
            Red = new Color(0xff0000ff);
            CornflowerBlue = new Color(0xffed9564);
            White = new Color(uint.MaxValue);
            Fuchsia = new Color(0xffff00ff);
            Lime = new Color(0xff00ff00);
            LightBlue = new Color(0xffe6d8ad);
            Teal = new Color(0xff808000);
            Orange = new Color(0xff00a5ff);
        }

        [CLSCompliant(false)]
        public Color(uint packedValue)
        {
            _packedValue = packedValue;
        }

        public Color(byte r,byte g,byte b,byte a)
        {
            _packedValue = ((uint)a << 24) | ((uint)b << 16) | ((uint)g << 8) | (r);
        }


        [DataMember]
        public byte G
        {
            get
            {
                unchecked
                {
                    return (byte)(this._packedValue >> 8);
                }
            }
            set
            {
                this._packedValue = (this._packedValue & 0xffff00ff) | ((uint)value << 8);
            }
        }

        [DataMember]
        public byte R
        {
            get { unchecked { return (byte)this._packedValue; } }
            set
            {
                _packedValue = (this._packedValue & 0xffffff00) | value;
            }
        }

        [DataMember]
        public byte A
        {
            get { unchecked { return (byte)(this._packedValue >> 24); } }
            set
            {
                this._packedValue = (this._packedValue & 0x00ffffff) | ((uint)value << 24);
            }
        }

        [DataMember]
        public byte B
        {
            get { unchecked
                {
                    return (byte)(this._packedValue >> 16);
                } }
            set
            {
                this._packedValue = (this._packedValue & 0xff00ffff) | ((uint)value << 16);
            }
        }

        public Vector4 ToVector4()
        {
            return new Vector4(R/255.0f,G/255.0f,B/255.0f,A/255.0f);
        }

        public Vector3 ToVector3()
        {
            return new Vector3(R / 255.0f, G / 255.0f, B / 255.0f);
        }



        public static Color Black
        {
            get;private set;
        }

        public static Color White
        {
            get;private set;
        }

        /// <summary>
        /// Red Color (R:255,G:0,B:0,A:255)
        /// </summary>
        public static Color Red { get; private set; }

        /// <summary>
        /// Green Color(R:0,G:128,B:0,A:255)
        /// </summary>
        public static Color Green { get; private set; }


        public static Color Blue { get; private set; }


        public static Color Yellow { get; private set; }
        
        public static Color CornflowerBlue { get; private set; }

        public static Color Fuchsia { get; private set; }

        public static Color Lime { get; private set; }

        public static Color LightBlue { get; private set; }

        public static Color Teal { get; private set; }

        public static Color Orange { get; private set; }

        public bool Equals(Color other)
        {
            return this.PackedValue == other.PackedValue;
        }


        public static bool operator !=(Color a,Color b)
        {
            return a._packedValue != b._packedValue;
        }

        public static bool operator ==(Color a,Color b)
        {
            return a._packedValue == b._packedValue;
        }


        public override int GetHashCode()
        {
            return this._packedValue.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return (obj is Color) && this.Equals((Color)obj);
        }

        private static void CheckARGBValues(int alpha,int red,int green,int blue)
        {
            if (alpha > 255 || (alpha < 0))
                throw new ArgumentException("alpha", alpha.ToString());
            CheckRGBValues(red, green, blue);
        }

        private static void CheckRGBValues(int red,int green,int blue)
        {
            if (red > 255 || red < 0)
                throw new ArgumentException("red", red.ToString());

            if (green > 255 || green < 0)
                throw new ArgumentException("green", green.ToString());

            if (blue > 255 || blue < 0)
                throw new ArgumentException("blue", blue.ToString());


        }


        public static Color FromArgb(int alpha,int red,int green,int blue)
        {
            CheckARGBValues(alpha, red, green, blue);
            return new Color((uint)(alpha << 24 + red << 16 + green << 8 + blue));
        }

        public static Color FromArgb(int argb)
        {
            return FromArgb((argb >> 24) & 0x0FF, (argb >> 16) & 0x0FF, (argb >> 8) & 0x0FF, argb & 0x0FF);
        }

        public int ToArgb()
        {
            return (int)_packedValue;
        }

        public float GetHue()
        {
            int r = R;
            int g = G;
            int b = B;
            byte minval = (byte)Math.Min(r, Math.Min(g, b));
            byte maxval = (byte)Math.Max(r, Math.Max(g, b));

            if (maxval == minval)
                return 0.0f;

            float diff = (float)(maxval - minval);
            float rnorm = (maxval - r) / diff;
            float gnorm = (maxval - g) / diff;
            float bnorm = (maxval - b) / diff;

            float hue = 0.0f;
            if (r == maxval)
                hue = 60.0f * (6.0f + bnorm - gnorm);
            if (g == maxval)
                hue = 60.0f * (2.0f + rnorm - bnorm);
            if (b == maxval)
                hue = 60.0f * (4.0f + gnorm - rnorm);
            if (hue > 360.0f)
                hue = hue - 360.0f;

            return hue;
        }

        public float GetBrightness()
        {
            byte minval = Math.Min(R, Math.Min(G, B));
            byte maxval = Math.Max(R, Math.Max(G, B));

            return (float)(maxval + minval) / 510;
        }

        public float GetSaturation()
        {
            byte minval = (byte)Math.Min(R, Math.Min(G, B));
            byte maxval = (byte)Math.Max(R, Math.Max(G, B));

            if (maxval == minval)
                return 0.0f;

            int sum = maxval + minval;
            if (sum > 255)
                sum = 510 - sum;

            return (float)(maxval - minval) / sum;
        }

    }
}