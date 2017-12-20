using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class ProductionQueueInfo : ITraitInfo
    {
        public virtual object Create(ActorInitializer init) { return new ProductionQueue(init,init.Self,this); } 
    }
    public class ProductionQueue
    {


        public ProductionQueue(ActorInitializer init,Actor playerActor,ProductionQueueInfo info)
        {

        }
    }
}