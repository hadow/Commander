using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class ClassicProductionQueueInfo : ProductionQueueInfo, Requires<TechTreeInfo>, Requires<PowerManagerInfo>, Requires<PlayerResourcesInfo>
    {
        public readonly bool SpeedUp = false;

        public override object Create(ActorInitializer init)
        {
            return base.Create(init);
        }
    }
    public class ClassicProductionQueue:ProductionQueue
    {
        readonly Actor self;
        readonly ClassicProductionQueueInfo info;

        public ClassicProductionQueue(ActorInitializer init,ClassicProductionQueueInfo info) : base(init, init.Self, info)
        {
            self = init.Self;
            this.info = info;
        }
    }
}