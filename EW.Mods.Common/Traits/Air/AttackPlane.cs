using System;
using EW.Activities;
using EW.Mods.Common.Activities;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class AttackPlaneInfo : AttackFrontalInfo, Requires<AircraftInfo>
    {
        [Desc("Delay,in game ticks,before turning to attack.")]
        public readonly int AttackTurnDelay = 50;

        public override object Create(ActorInitializer init)
        {
            return new AttackPlane(init.Self,this);
        }
    }
    public class AttackPlane:AttackFrontal
    {
        public readonly AttackPlaneInfo AttackPlaneInfo;
        readonly AircraftInfo aircraftInfo;
        public AttackPlane(Actor self,AttackPlaneInfo info):base(self,info)
        {
            AttackPlaneInfo = info;
            aircraftInfo = self.Info.TraitInfo<AircraftInfo>();
        }

        public override Activity GetAttackActivity(Actor self, Target newTarget, bool allowMove, bool forceAttack)
        {
            return new FlyAttack(self, newTarget);
        }

        protected override bool CanAttack(Actor self, Target target)
        {
            //Don't fire while landed or when outside the map
            return base.CanAttack(self, target)
                && self.World.Map.DistanceAboveTerrain(self.CenterPosition).Length >= aircraftInfo.MinAirborneAltitude
                && self.World.Map.Contains(self.Location);
        }
    }
}