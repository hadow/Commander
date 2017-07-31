using System;

namespace EW
{
    public struct WRot:IEquatable<WRot>
    {

        public readonly WAngle Pitch, Roll, Yaw;

        public WRot(WAngle pitch,WAngle roll,WAngle yaw)
        {
            this.Pitch = pitch;
            this.Roll = roll;
            this.Yaw = yaw;

        }

        public static bool operator ==(WRot a,WRot b)
        {
            return a.Pitch == b.Pitch && a.Roll == b.Roll && a.Yaw == b.Yaw;
        }

        public static bool operator !=(WRot a,WRot b)
        {
            return !(a == b);
        }
        public override int GetHashCode()
        {
            return Pitch.GetHashCode() ^ Roll.GetHashCode() ^ Yaw.GetHashCode();
        }

        public bool Equals(WRot other)
        {
            return this == other;
        }

        public override bool Equals(object obj)
        {
            return (obj is WRot) && Equals((WRot)obj);
        }
    }
}