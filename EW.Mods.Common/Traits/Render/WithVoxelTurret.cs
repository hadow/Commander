using System;
using System.Linq;
using System.Collections.Generic;
using EW.Traits;
using EW.Graphics;
namespace EW.Mods.Common.Traits.Render
{
    public class WithVoxelTurretInfo:ConditionalTraitInfo,Requires<RenderVoxelsInfo>,Requires<TurretedInfo>
    {
        public readonly string Sequence = "turret";

        public readonly string Turret = "primary";

        public readonly bool ShowShadow = true;
        

        public override object Create(ActorInitializer init)
        {
            return new WithVoxelTurret(init.Self, this);
        }
    }


    public class WithVoxelTurret : ConditionalTrait<WithVoxelTurretInfo>,INotifyBuildComplete
    {

        readonly Actor self;
        readonly Turreted turreted;
        readonly BodyOrientation body;

        bool buildComplete;

        public WithVoxelTurret(Actor self,WithVoxelTurretInfo info):base(info)
        {
            this.self = self;
            body = self.Trait<BodyOrientation>();
            turreted = self.TraitsImplementing<Turreted>().First(tt => tt.Name == Info.Turret);
            buildComplete = !self.Info.HasTraitInfo<BuildingInfo>();//always render instantly for units.

            var rv = self.Trait<RenderVoxels>();
            rv.Add(new ModelAnimation(self.World.ModelCache.GetModelSequence(rv.Image, Info.Sequence),
                () => turreted.Position(self), TurretRotation, () => IsTraitDisabled || !buildComplete, () => 0, info.ShowShadow));
        }


        IEnumerable<WRot> TurretRotation()
        {
            var b = self.Orientation;
            var qb = body.QuantizeOrientation(self, b);
            yield return turreted.WorldOrientation(self) - b + WRot.FromYaw(b.Yaw - qb.Yaw);
            yield return qb;
        }

        void INotifyBuildComplete.BuildingComplete(Actor self) { buildComplete = true; }
    }
}