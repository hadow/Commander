using System;
using System.Drawing;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    [Desc("Plays an audio notification and shows a radar ping when a building is attacked.")]
    public class BaseAttackNotifierInfo : ITraitInfo
    {

        public readonly int NotifyInterval = 30;

        public readonly Color RadarPingColor = Color.Red;

        public readonly int RadarPingDuration = 10 * 25;

        public string Notification = "BaseAttack";

        public string AllyNotification = null;


        public object Create(ActorInitializer init) { return new BaseAttackNotifier(init.Self,this); }
    }
    public class BaseAttackNotifier:INotifyDamage
    {
        readonly RadarPings radarPings;
        readonly BaseAttackNotifierInfo info;

        int lastAttackTime;

        public BaseAttackNotifier(Actor self,BaseAttackNotifierInfo info)
        {
            radarPings = self.World.WorldActor.TraitOrDefault<RadarPings>();
            this.info = info;
            lastAttackTime = -info.NotifyInterval * 25;
        }

        void INotifyDamage.Damaged(Actor self, AttackInfo attackInfo)
        {
            if (attackInfo == null)
                return;

            if (attackInfo.Attacker.Owner == self.Owner)
                return;

            if (attackInfo.Attacker == self.World.WorldActor)
                return;

            if (!self.Info.HasTraitInfo<BuildingInfo>())
                return;

            if (attackInfo.Attacker.Owner.IsAlliedWith(self.Owner) && attackInfo.Damage.Value <= 0)
                return;


            if(self.World.WorldTick - lastAttackTime > info.NotifyInterval * 25)
            {
                var rules = self.World.Map.Rules;
                WarGame.Sound.PlayNotification(rules, self.Owner, "Speech", info.Notification, self.Owner.Faction.InternalName);

                if (info.AllyNotification != null)
                    foreach (var p in self.World.Players)
                        if (p != self.Owner && p.IsAlliedWith(self.Owner) && p != attackInfo.Attacker.Owner)
                            WarGame.Sound.PlayNotification(rules, p, "Speech", info.AllyNotification, p.Faction.InternalName);

                if (radarPings != null)
                    radarPings.Add(() => self.Owner.IsAlliedWith(self.World.RenderPlayer), self.CenterPosition, info.RadarPingColor, info.RadarPingDuration);
            }

            lastAttackTime = self.World.WorldTick;
        }
       


    }
}