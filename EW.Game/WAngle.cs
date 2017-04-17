using System;


namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public struct  WAngle:IEquatable<WAngle>
    {
        public readonly int Angle;

        public static bool operator ==(WAngle me,WAngle other) { return me.Angle == other.Angle; }

        public  static bool operator !=(WAngle me,WAngle other) { return me.Angle != other.Angle; }

        public bool Equals(WAngle other) { return this == other; }
    }
}