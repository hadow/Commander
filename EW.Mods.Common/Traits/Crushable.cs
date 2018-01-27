using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// This actor is crushable.
    /// </summary>
    class CrushableInfo : ConditionalTraitInfo
    {

        public readonly string CrushSound = null;

        public readonly HashSet<string> CrushClasses = new HashSet<string> { "infantry" };

        /// <summary>
        /// Probability of mobile actors noticing and evading a crush attemp.
        /// </summary>
        public readonly int WarnProbability = 75;

        /// <summary>
        /// Will friendly units just crush me instead of pathing around.
        /// 友军单位是否可以穿越我，而不是四处走动
        /// </summary>
        public readonly bool CrushedByFriendlies = false;

        public override object Create(ActorInitializer init)
        {
            return new Crushable(init.Self, this);
        }
    }
    class Crushable:ConditionalTrait<CrushableInfo>,ICrushable,INotifyCrushed
    {

        readonly Actor self;

        public Crushable(Actor self,CrushableInfo info) : base(info)
        {
            this.self = self;
        }

        void INotifyCrushed.WarnCrush(Actor self, Actor crusher, HashSet<string> crushClasses)
        {
            if (!CrushableInner(crushClasses, crusher.Owner))
                return;


            var mobile = self.TraitOrDefault<Mobile>();
            if (mobile != null && self.World.SharedRandom.Next(100) <= Info.WarnProbability)
                mobile.Nudge(self, crusher, true);
        }


        void INotifyCrushed.OnCrush(Actor self, Actor crusher, HashSet<string> crushClasses)
        {
            if (!CrushableInner(crushClasses, crusher.Owner))
                return;
            WarGame.Sound.Play(SoundType.World, Info.CrushSound, crusher.CenterPosition);

            self.Kill(crusher);

        }

        bool ICrushable.CrushableBy(Actor self, Actor crusher, HashSet<string> crushClasses)
        {
            return CrushableInner(crushClasses, crusher.Owner);
        }

        bool CrushableInner(HashSet<string> crushClasses,Player crushOwner)
        {
            if (IsTraitDisabled)
                return false;

            if (!self.IsAtGroundLevel())
                return false;

            if (!Info.CrushedByFriendlies && crushOwner.IsAlliedWith(self.Owner))
                return false;

            return Info.CrushClasses.Overlaps(crushClasses);
        }

    }
}