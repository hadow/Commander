using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class AttackTurretedInfo : AttackFollowInfo, Requires<TurretedInfo>
    {
        public override object Create(ActorInitializer init)
        {
            return new AttackTurreted(init.Self, this);
        }
    }
    public class AttackTurreted:AttackFollow
    {


        public AttackTurreted(Actor self,AttackTurretedInfo info) : base(self, info) { }
    }
}