using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{


    public class BodyOrientationInfo:ITraitInfo
    {

        public readonly int QuantizedFacings = -1;

        public readonly WAngle CameraPitch = WAngle.FromDegrees((40));

        public readonly bool UseClassicPerspectiveFudge = true;

        public readonly bool UseClassicFacingFudge = false;

        public WVect LocalToWorld(WVect vec){
            if (!UseClassicPerspectiveFudge)
                return new WVect(vec.Y, -vec.X, vec.Z);
            return new WVect(vec.Y, -CameraPitch.Sin() * vec.X / 1024, vec.Z);
        }


        public object Create(ActorInitializer init){
            return new BodyOrientation(init, this);
        }
    }
    public class BodyOrientation:ISync
    {

        readonly BodyOrientationInfo info;
        readonly Lazy<int> quantizedFacings;
        public BodyOrientation(ActorInitializer init, BodyOrientationInfo info)
        {

            this.info = info;

        }
    }
}
