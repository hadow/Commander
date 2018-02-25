using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class ClassicProductionQueueInfo : ProductionQueueInfo, Requires<TechTreeInfo>, Requires<PowerManagerInfo>, Requires<PlayerResourcesInfo>
    {
        public readonly bool SpeedUp = false;


        public readonly int[] BuildTimeSpeedReduction = { 100, 85, 75, 65, 60, 55, 50 };

        public override object Create(ActorInitializer init)
        {
            return new ClassicProductionQueue(init, this);
        }
    }
    public class ClassicProductionQueue:ProductionQueue
    {
        static readonly ActorInfo[] NoItems = { };


        readonly Actor self;
        readonly ClassicProductionQueueInfo info;

        public ClassicProductionQueue(ActorInitializer init,ClassicProductionQueueInfo info) : base(init, init.Self, info)
        {
            self = init.Self;
            this.info = info;
        }


        protected override void Tick(Actor self)
        {

        }

    }
}