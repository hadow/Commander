using System;
using System.Collections.Generic;
using EW.Traits;
using System.Linq;
namespace EW.Mods.Common.Traits
{
    public class ProvidesPrerequisiteInfo : ConditionalTraitInfo,ITechTreePrerequisiteInfo
    {
        public readonly string Prerequisite = null;

        public readonly string[] RequiresPrerequisites = { };

        public readonly HashSet<string> Factions = new HashSet<string>();

        public readonly bool ResetOnOwnerChange = false;


        public override object Create(ActorInitializer init)
        {
            return new ProvidesPrerequisite(init, this);
        }
    }
    public class ProvidesPrerequisite:ConditionalTrait<ProvidesPrerequisiteInfo>,ITechTreePrerequisite,INotifyOwnerChanged,INotifyCreated
    {
        readonly string prerequisite;

        bool enabled;
        TechTree techTree;
        string faction;

        public ProvidesPrerequisite(ActorInitializer init,ProvidesPrerequisiteInfo info) : base(info)
        {
            prerequisite = info.Prerequisite;

            if (string.IsNullOrEmpty(prerequisite))
                prerequisite = init.Self.Info.Name;

            faction = init.Contains<FactionInit>() ? init.Get<FactionInit, string>() : init.Self.Owner.Faction.InternalName;

        }

        public IEnumerable<string> ProvidesPrerequisites
        {
            get
            {
                if (!enabled)
                    yield break;

                yield return prerequisite;
            }
        }

        protected override void Created(Actor self)
        {

            var playerActor = self.Info.Name == "player" ? self : self.Owner.PlayerActor;

            techTree = playerActor.Trait<TechTree>();

            

            base.Created(self);
        }

        void Update()
        {
            enabled = !IsTraitDisabled;
            if (IsTraitDisabled)
                return;

            if (Info.Factions.Any())
                enabled = Info.Factions.Contains(faction);

            if (Info.RequiresPrerequisites.Any() && enabled)
                enabled = techTree.HasPrerequisites(Info.RequiresPrerequisites);
        }

        protected override void TraitEnabled(Actor self)
        {
            Update();
            techTree.ActorChanged(self);
        }

        protected override void TraitDisabled(Actor self)
        {
            Update();
            techTree.ActorChanged(self);
        }


        void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner)
        {
            techTree = newOwner.PlayerActor.Trait<TechTree>();

            if (Info.ResetOnOwnerChange)
                faction = newOwner.Faction.InternalName;

            Update();
        }



    }
}