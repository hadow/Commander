using System;
using System.Collections.Generic;
using EW.Traits;

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
    public class GrantConditionOnPrerequisite:INotifyCreated,INotifyAddToWorld,INotifyRemovedFromWorld,INotifyOwnerChanged
    {
        public GrantConditionOnPrerequisite(Actor self,GrantConditionOnPrerequisiteInfo info)
        {
            
        }


        void INotifyCreated.Created(Actor self)
        {

        }

        void INotifyAddToWorld.AddedToWorld(Actor self) { }

        void INotifyOwnerChanged.OnOwnerChanged(Actor self, Player oldOwner, Player newOwner) { }

        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
        {

        }





    }
}