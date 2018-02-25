using System;
using System.Collections.Generic;
using EW.Traits;
using System.Linq;

namespace EW.Mods.Common.Traits
{

    public class GrantConditionOnPrerequisiteInfo : ITraitInfo
    {
        public readonly string Condition = null;

        public readonly string[] Prerequisites = { };

        public object Create(ActorInitializer init)
        {
            return new GrantConditionOnPrerequisite(init.Self,this);
        }
    }
    public class GrantConditionOnPrerequisite:INotifyCreated,INotifyAddedToWorld,INotifyRemovedFromWorld,INotifyOwnerChanged
    {
        readonly GrantConditionOnPrerequisiteInfo info;

        bool wasAvailable;
        ConditionManager conditionManager;
        GrantConditionOnPrerequisiteManager globalManager;
        int conditionToken = ConditionManager.InvalidConditionToken;



        public GrantConditionOnPrerequisite(Actor self,GrantConditionOnPrerequisiteInfo info)
        {
            this.info = info;
        }


        void INotifyCreated.Created(Actor self)
        {
            var playerActor = self.Info.Name == "player" ? self : self.Owner.PlayerActor;

            globalManager = playerActor.Trait<GrantConditionOnPrerequisiteManager>();
            conditionManager = self.TraitOrDefault<ConditionManager>();

        }

        void INotifyAddedToWorld.AddedToWorld(Actor self)
        {
            if (info.Prerequisites.Any())
                globalManager.Register(self, this, info.Prerequisites);
        }

        void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner) { }

        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
        {
            if (info.Prerequisites.Any())
                globalManager.Unregister(self, this, info.Prerequisites);
        }


        public void PrerequisitesUpdated(Actor self,bool available)
        {

        }





    }
}