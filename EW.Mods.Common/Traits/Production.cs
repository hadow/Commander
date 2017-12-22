using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class ProductionInfo : ITraitInfo
    {

        public virtual object Create(ActorInitializer init) { return new Production(init,this); }
    }


    public class Production:INotifyCreated
    {
        public readonly ProductionInfo Info;
        public Production(ActorInitializer init,ProductionInfo info)
        {
            this.Info = info;

        }


        void INotifyCreated.Created(Actor self)
        {

        }


        public virtual bool Produce(Actor self,ActorInfo producee,string factionVariant)
        {
            return false;
        }
    }
}