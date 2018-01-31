using System;
using EW.Traits;
using System.Drawing;

namespace EW.Mods.Common.Traits
{

    public class HarvesterAttackNotifierInfo : ITraitInfo
    {
        public readonly int NotifyInterval = 30;

        public readonly Color RadarPingColor = Color.Red;

        public readonly int RadarPingDuration = 10 * 25;

        public string Notification = "HarvesterAttack";

        public object Create(ActorInitializer init)
        {
            return new HarvesterAttackNotifier(init.Self,this);
        }
    }


    public class HarvesterAttackNotifier:INotifyDamage
    {
        readonly RadarPings radarPings;

        readonly HarvesterAttackNotifierInfo info;

        int lastAttackTime;

        public HarvesterAttackNotifier(Actor self,HarvesterAttackNotifierInfo info)
        {
            radarPings = self.World.WorldActor.TraitOrDefault<RadarPings>();
            this.info = info;
            lastAttackTime = -info.NotifyInterval * 25;
        }


        void INotifyDamage.Damaged(Actor self, AttackInfo attackInfo)
        {
            //Don't track self-damage
            if (attackInfo.Attacker != null && attackInfo.Attacker.Owner == self.Owner)
                return;

            //Only track last hit against our harvesters
            if (!self.Info.HasTraitInfo<HarvesterInfo>())
                return;

            if(self.World.WorldTick - lastAttackTime > info.NotifyInterval * 25)
            {
                WarGame.Sound.PlayNotification(self.World.Map.Rules, self.Owner, "Speech", info.Notification, self.Owner.Faction.InternalName);

                if (radarPings != null)
                    radarPings.Add(() => self.Owner.IsAlliedWith(self.World.RenderPlayer), self.CenterPosition, info.RadarPingColor, info.RadarPingDuration);
            }

            lastAttackTime = self.World.WorldTick;
        }
    }
}