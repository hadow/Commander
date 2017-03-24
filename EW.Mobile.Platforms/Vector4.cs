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


        public bool Equals(Vector4 other)
        {
            return this.W == other.W && this.X == other.X && this.Y == other.Y && this.Z == other.Z;
        }
    }
}