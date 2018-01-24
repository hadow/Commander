using System;

namespace EW
{
    /// <summary>
    /// 3D World rotation.
    /// </summary>
    public struct WRot:IEquatable<WRot>
    {
        public static readonly WRot Zero = new WRot(WAngle.Zero, WAngle.Zero, WAngle.Zero);
        public readonly WAngle Pitch, Roll, Yaw;

        public WRot(WAngle roll,WAngle pitch,WAngle yaw)
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
        public static WRot operator +(WRot a,WRot b) { return new WRot(a.Roll + b.Roll, a.Pitch + b.Pitch, a.Yaw + b.Yaw); }
        public static WRot operator -(WRot a,WRot b) { return new WRot(a.Roll - b.Roll, a.Pitch - b.Pitch, a.Yaw - b.Yaw); }
        public static WRot operator -(WRot a) { return new WRot(-a.Roll, -a.Pitch, -a.Yaw); }

        public static WRot FromFacing(int facing)
        {
            return new WRot(WAngle.Zero, WAngle.Zero, WAngle.FromFacing(facing));
        }

        public static WRot FromYaw(WAngle yaw) { return new WRot(WAngle.Zero, WAngle.Zero, yaw); }
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

        public int[] AsQuarternion()
        {
            // Angles increase clockwise
            var r = new WAngle(-Roll.Angle / 2);
            var p = new WAngle(-Pitch.Angle / 2);
            var y = new WAngle(-Yaw.Angle / 2);
            var cr = (long)r.Cos();
            var sr = (long)r.Sin();
            var cp = (long)p.Cos();
            var sp = (long)p.Sin();
            var cy = (long)y.Cos();
            var sy = (long)y.Sin();

            // Normalized to 1024 == 1.0
            return new int[4]
            {
                (int)((sr * cp * cy - cr * sp * sy) / 1048576), // x
				(int)((cr * sp * cy + sr * cp * sy) / 1048576), // y
				(int)((cr * cp * sy - sr * sp * cy) / 1048576), // z
				(int)((cr * cp * cy + sr * sp * sy) / 1048576)  // w
			};
        }

        public int[] AsMatrix(){

            var q = AsQuarternion();

            // Theoretically 1024 *  * 2, but may differ slightly due to rounding
            var lsq = q[0] * q[0] + q[1] * q[1] + q[2] * q[2] + q[3] * q[3];

            // Quaternion components use 10 bits, so there's no risk of overflow
            var mtx = new int[16];
            mtx[0] = lsq - 2 * (q[1] * q[1] + q[2] * q[2]);
            mtx[1] = 2 * (q[0] * q[1] + q[2] * q[3]);
            mtx[2] = 2 * (q[0] * q[2] - q[1] * q[3]);
            mtx[3] = 0;

            mtx[4] = 2 * (q[0] * q[1] - q[2] * q[3]);
            mtx[5] = lsq - 2 * (q[0] * q[0] + q[2] * q[2]);
            mtx[6] = 2 * (q[1] * q[2] + q[0] * q[3]);
            mtx[7] = 0;

            mtx[8] = 2 * (q[0] * q[2] + q[1] * q[3]);
            mtx[9] = 2 * (q[1] * q[2] - q[0] * q[3]);
            mtx[10] = lsq - 2 * (q[0] * q[0] + q[1] * q[1]);
            mtx[11] = 0;

            mtx[12] = 0;
            mtx[13] = 0;
            mtx[14] = 0;
            mtx[15] = lsq;

            return mtx;
        }


    }
}