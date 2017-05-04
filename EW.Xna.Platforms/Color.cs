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

        public static Color CornflowerBlue { get; private set; }
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
    }
}