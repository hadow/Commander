using System;
using System.Linq;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// Actor has a visual turret used to attack.
    /// </summary>
    public class AttackTurretedInfo : AttackFollowInfo, Requires<TurretedInfo>
    {
        public readonly string[] Turrets = { "primary" };

        public override object Create(ActorInitializer init)
        {
            return new AttackTurreted(init.Self, this);
        }
    }
    public class AttackTurreted:AttackFollow
    {
        protected Turreted[] turrets;

        public AttackTurreted(Actor self,AttackTurretedInfo info) : base(self, info)
        {
            turrets = self.TraitsImplementing<Turreted>().Where(t => info.Turrets.Contains(t.Info.Turret)).ToArray();
        }

        protected override bool CanAttack(Actor self, Target target)
        {
            if (target.Type == TargetT.Invalid)
                return false;
            //Don't break early from this loop - we want to
            var turretReady = false;
            foreach (var t in turrets)
                if (t.FaceTarget(self, target))
                    turretReady = true;

            return turretReady && base.CanAttack(self, target);
        }
    }
}