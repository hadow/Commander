using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class BodyOrientationInfo:ITraitInfo
    {
        /// <summary>
        /// Number of facings for gameplay calculations.
        /// -1 indicates auto-detection from another trait
        /// </summary>
        public readonly int QuantizedFacings = -1;

        /// <summary>
        /// Camera pitch for roation calculations
        /// </summary>
        public readonly WAngle CameraPitch = WAngle.FromDegrees((40));

        /// <summary>
        /// 
        /// </summary>
        public readonly bool UseClassicPerspectiveFudge = true;

        public readonly bool UseClassicFacingFudge = false;

        public WVec LocalToWorld(WVec vec)
        {
            //Rotate by 90 degrees
            if (!UseClassicPerspectiveFudge)
                return new WVec(vec.Y, -vec.X, vec.Z);

            return new WVec(vec.Y, -CameraPitch.Sin() * vec.X / 1024, vec.Z);
        }


        public WRot QuantizeOrientation(WRot orientation, int facings)
        {

            //Quantization disabled
            if (facings == 0)
                return orientation;

            var facing = QuantizeFacing(orientation.Yaw.Angle / 4, facings);

            return new WRot(WAngle.Zero, WAngle.Zero, WAngle.FromFacing(facings));

        }

        public int QuantizeFacing(int facing, int facings){

            return Util.QuantizeFacing(facing, facings) * (256 / facing);
        }

        public object Create(ActorInitializer init){
            return new BodyOrientation(init, this);
        }
    }

    public class BodyOrientation:ISync
    {

        readonly BodyOrientationInfo info;
        readonly Lazy<int> quantizedFacings;

        [Sync]
        public int QuantizedFacings { get { return quantizedFacings.Value; } }
        public BodyOrientation(ActorInitializer init, BodyOrientationInfo info)
        {

            this.info = info;
            var self = init.Self;
            var faction = init.Contains<FactionInit>() ? init.Get<FactionInit, string>() : self.Owner.Faction.InternalName;
        }

        public WAngle CameraPitch { get { return info.CameraPitch; } }

        /// <summary>
        /// Locals to world.
        /// </summary>
        /// <returns>The to world.</returns>
        /// <param name="vec">Vec.</param>
        public WVec LocalToWorld(WVec vec){
            return info.LocalToWorld(vec);
        }


        public WRot QuantizeOrientation(Actor self,WRot orientation){
            return info.QuantizeOrientation(orientation, quantizedFacings.Value);
        }


    }
}
