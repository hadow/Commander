using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    class KillsSelfInfo:ConditionalTraitInfo
    {

        [Desc("Remove the actor from the world (and destroy it) instead of killing it.")]
        public readonly bool RemoveInstead = false;

        [Desc("The amount of time (in ticks) before the actor dies. Two values indicate a range between which a random value is chosen.")]
        public readonly int[] Delay = { 0 };

        [GrantedConditionReference]
        [Desc("The condition to grant moments before suiciding.")]
        public readonly string GrantsCondition = null;

        public override object Create(ActorInitializer init)
        {
            return new KillsSelf(init.Self, this);
        }

    }

    class KillsSelf : ConditionalTrait<KillsSelfInfo>,INotifyCreated,INotifyAddedToWorld,ITick
    {
        int lifetime;
        ConditionManager conditionManager;


        public KillsSelf(Actor self,KillsSelfInfo info) : base(info)
        {
            lifetime = Util.RandomDelay(self.World, info.Delay);

        }

        protected override void TraitEnabled(Actor self)
        {
            // Actors can be created without being added to the world
            // We want to make sure that this only triggers once they are inserted into the world
            if (lifetime == 0 && self.IsInWorld)
                Kill(self);
        }

        void INotifyAddedToWorld.AddedToWorld(Actor self)
        {
            if (!IsTraitDisabled)
                TraitEnabled(self);
        }

        void ITick.Tick(Actor self)
        {
            if (!self.IsInWorld || self.IsDead || IsTraitDisabled)
                return;

            if (!self.World.Map.Contains(self.Location))
                return;

            if (lifetime-- <= 0)
                Kill(self);
        }

        void Kill(Actor self)
        {
            if (self.IsDead)
                return;

            if (conditionManager != null && !string.IsNullOrEmpty(Info.GrantsCondition))
                conditionManager.GrantCondition(self, Info.GrantsCondition);

            if (Info.RemoveInstead || !self.Info.HasTraitInfo<HealthInfo>())
                self.Dispose();
            else
                self.Kill(self);
        }
    }
}