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
    }
}