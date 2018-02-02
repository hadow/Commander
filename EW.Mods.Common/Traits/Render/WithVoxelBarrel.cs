using System;
using EW.Traits;
using EW.Graphics;
using System.Linq;
using System.Collections.Generic;

namespace EW.Mods.Common.Traits.Render
{
    public class WithVoxelBarrelInfo:ConditionalTraitInfo,Requires<RenderVoxelsInfo>,Requires<ArmamentInfo>,Requires<TurretedInfo>
    {

        [Desc("Voxel sequence name to use")]
        public readonly string Sequence = "barrel";

        [Desc("Armament to use for recoil")]
        public readonly string Armament = "primary";

        [Desc("Visual offset")]
        public readonly WVec LocalOffset = WVec.Zero;

        [Desc("Rotate the barrel relative to the body")]
        public readonly WRot LocalOrientation = WRot.Zero;

        [Desc("Defines if the Voxel should have a shadow.")]
        public readonly bool ShowShadow = true;
        public override object Create(ActorInitializer init)
        {
            return new WithVoxelBarrel(init.Self, this);
        }

    }


    public class WithVoxelBarrel : ConditionalTrait<WithVoxelBarrelInfo>,INotifyBuildComplete
    {
        readonly Actor self;
        readonly Armament armament;
        readonly Turreted turreted;
        readonly BodyOrientation body;

        // TODO: This should go away once https://github.com/OpenRA/OpenRA/issues/7035 is implemented
        bool buildComplete;

        public WithVoxelBarrel(Actor self, WithVoxelBarrelInfo info)
            : base(info)
        {
            this.self = self;
            body = self.Trait<BodyOrientation>();
            armament = self.TraitsImplementing<Armament>()
                .First(a => a.Info.Name == Info.Armament);
            turreted = self.TraitsImplementing<Turreted>()
                .First(tt => tt.Name == armament.Info.Turret);

            buildComplete = !self.Info.HasTraitInfo<BuildingInfo>(); // always render instantly for units

            var rv = self.Trait<RenderVoxels>();
            rv.Add(new ModelAnimation(self.World.ModelCache.GetModelSequence(rv.Image, Info.Sequence),
                BarrelOffset, BarrelRotation,
                () => IsTraitDisabled || !buildComplete, () => 0, info.ShowShadow));
        }

        WVec BarrelOffset()
        {
            var b = self.Orientation;
            var qb = body.QuantizeOrientation(self, b);
            var localOffset = Info.LocalOffset + new WVec(-armament.Recoil, WDist.Zero, WDist.Zero);
            var turretLocalOffset = turreted != null ? turreted.Offset : WVec.Zero;
            var turretOrientation = turreted != null ? turreted.WorldOrientation(self) - b + WRot.FromYaw(b.Yaw - qb.Yaw) : WRot.Zero;

            return body.LocalToWorld((turretLocalOffset + localOffset.Rotate(turretOrientation)).Rotate(qb));
        }

        IEnumerable<WRot> BarrelRotation()
        {
            var b = self.Orientation;
            var qb = body.QuantizeOrientation(self, b);
            yield return Info.LocalOrientation;
            yield return turreted.WorldOrientation(self) - b + WRot.FromYaw(b.Yaw - qb.Yaw);
            yield return qb;
        }

        void INotifyBuildComplete.BuildingComplete(Actor self) { buildComplete = true; }
    }
}