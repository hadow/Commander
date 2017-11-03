using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class GrantConditionOnDisabledInfo:ITraitInfo
    {

        public object Create(ActorInitializer init) { return new GrantedConditionDisabled(); }

    }


    public class GrantedConditionDisabled : INotifyCreated, ITick
    {
        void INotifyCreated.Created(Actor self)
        {

        }

        public void Tick(Actor self)
        {

        }
    }
}