using System;
using EW.Mods.Common.Activities;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class AttackPlaneInfo : AttackFrontalInfo, Requires<AircraftInfo>
    {
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

    }
}