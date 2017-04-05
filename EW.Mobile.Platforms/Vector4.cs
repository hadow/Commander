using System;
using System.Runtime.Serialization;
namespace EW.Mobile.Platforms
{
    public struct Vector4:IEquatable<Vector4>
    {

        [DataMember]
        public float X;
        [DataMember]
        public float Y;
        [DataMember]
        public float Z;
        [DataMember]
        public float W;
        
        private static readonly Vector4 zero = new Vector4();
        public static Vector4 Zero { get { return zero; } }

        public Vector4(float x,float y ,float z,float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }
        public bool Equals(Vector4 other)
        {
            return this.W == other.W && this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }


        #region Operators

        public static Vector4 operator -(Vector4 value)
        {
            return new Vector4(value.X, value.Y, value.Z, value.W);
        }

        public static bool operator ==(Vector4 value1,Vector4 value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y && value1.Z == value2.Z && value1.W == value2.W;
        }

        public static bool operator !=(Vector4 value1,Vector4 value2)
        {
            return !(value1 == value2);
        }
        
        public static Vector4 operator +(Vector4 value1,Vector4 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            value1.W += value2.W;
            return value1;
        }


        #endregion
    }
}