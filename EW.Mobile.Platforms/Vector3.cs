using System;
using System.Runtime.Serialization;
namespace EW.Mobile.Platforms
{
    /// <summary>
    /// 3D œÚ¡ø
    /// </summary>
    public struct Vector3:IEquatable<Vector3>
    {
        [DataMember]
        public float X;
        [DataMember]
        public float Y;
        [DataMember]
        public float Z;

        public Vector3(float x,float y,float z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public Vector3(float value)
        {
            X = value;
            Y = value;
            Z = value;
        }

        public Vector3(Vector2 v2,float value)
        {
            X = v2.X;
            Y = v2.Y;
            Z = value;
        }

        public bool Equals(Vector3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }
        public static bool operator !=(Vector3 value1,Vector3 value2)
        {
            return !(value1 == value2);
        }
        public static bool operator ==(Vector3 value1,Vector3 value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y && value1.Z == value2.Z;
        }

    }
}