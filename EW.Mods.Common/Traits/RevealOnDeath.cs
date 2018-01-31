using System;
using System.Collections.Generic;
using EW.Mods.Common.Effects;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class RevealOnDeathInfo : ConditionalTraitInfo
    {
        public readonly Stance RevealForStances = Stance.Ally;

        public readonly int Duration = 25;

        public readonly WDist Radius = new WDist(1536);

        public readonly bool RevealGeneratedShroud = true;

        /// <summary>
        /// DeathTypes for which shroud will be revealed.
        /// </summary>
        public readonly HashSet<string> DeathTypes = new HashSet<string>();

        public override object Create(ActorInitializer init)
        {
            return new RevealOnDeath(init.Self, this);
        }
    }

    public class RevealOnDeath:ConditionalTrait<RevealOnDeathInfo>,INotifyKilled
    {
        public RevealOnDeath(Actor self,RevealOnDeathInfo info) : base(info) { }


        void INotifyKilled.Killed(Actor self, AttackInfo attackInfo)
        {
            if (IsTraitDisabled)
                return;

            if (!self.IsInWorld)
                return;

            if (Info.DeathTypes.Count > 0 && !attackInfo.Damage.DamageTypes.Overlaps(Info.DeathTypes))
                return;

            var owner = self.Owner;
            if(owner != null && owner.WinState == WinState.Undefined)
            {
                self.World.AddFrameEndTask(w =>
                {

                    if (self.Disposed)
                        return;

                    w.Add(new RevealShroudEffect(self.CenterPosition, Info.Radius,
                        Info.RevealGeneratedShroud ? Shroud.SourceType.Visibility : Shroud.SourceType.PassiveVisibility, owner, Info.RevealForStances, duration: Info.Duration));

                });
            }
        }
    }
}