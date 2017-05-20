using System;
using System.Runtime.Serialization;
namespace EW.Xna.Platforms
{
    /// <summary>
    /// 3D œÚ¡ø
    /// </summary>
    public struct Vector3 : IEquatable<Vector3>
    {
        [DataMember]
        public float X;
        [DataMember]
        public float Y;
        [DataMember]
        public float Z;

        public Vector3(float x, float y, float z)
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

        public Vector3(Vector2 v2, float value)
        {
            X = v2.X;
            Y = v2.Y;
            Z = value;
        }

        #region Private Fields
        private static readonly Vector3 zero = new Vector3(0f, 0f, 0f);


        #endregion

        public bool Equals(Vector3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Vector3))
                return false;
            var other = (Vector3)obj;
            return X == other.X && Y == other.Y && Z == other.Z;
        }
        public static bool operator !=(Vector3 value1, Vector3 value2)
        {
            return !(value1 == value2);
        }
        public static bool operator ==(Vector3 value1, Vector3 value2)
        {
            return value1.X == value2.X && value1.Y == value2.Y && value1.Z == value2.Z;
        }

        public static Vector3 operator +(Vector3 value1, Vector3 value2)
        {
            value1.X += value2.X;
            value1.Y += value2.Y;
            value1.Z += value2.Z;
            return value1;
        }

        public static Vector3 operator -(Vector3 value1, Vector3 value2)
        {
            value1.X -= value2.X;
            value1.Y -= value2.Y;
            value1.Z -= value2.Z;
            return value1;
        }

        public static Vector3 operator /(Vector3 value, Vector3 val2)
        {
            value.X /= val2.X;
            value.Y /= val2.Y;
            value.Z /= val2.Z;
            return value;
        }

        public static Vector3 operator /(Vector3 value,float divider)
        {
            float factor = 1 / divider;
            return value * factor;
        }

        public static Vector3 operator -(Vector3 value)
        {
            return new Vector3(-value.X, -value.Y, -value.Z);
        }

        public static Vector3 operator *(Vector3 value1, Vector3 value2)
        {
            value1.X *= value2.X;
            value1.Y *= value2.Y;
            value1.Z *= value2.Z;
            return value1;
        }

        public static Vector3 operator *(Vector3 value1,float scaleFactor)
        {
            return new Vector3(value1.X * scaleFactor, value1.Y * scaleFactor, value1.Z * scaleFactor);
        }
            

        public static implicit operator Vector3(Vector2 src) { return new Vector3(src.X, src.Y, 0); }

        public static Vector3 Zero { get { return zero; } }

        //public override int GetHashCode()
        //{
        //    unchecked
        //    {
        //        var hashCode = X.GetHashCode();
        //        hashCode = (hashCode * 397) ^ Y.GetHashCode();
        //        hashCode = (hashCode * 397) ^ Z.GetHashCode();
        //        return hashCode;
        //    }
        //}

        public override int GetHashCode()
        {
            return (int)(this.X + this.Y + this.Z);
        }

    }
}