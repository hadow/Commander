using System;
using EW.Traits;
using System.Linq;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// Lets the actor spread resources around it in a circle.
    /// </summary>
    class SeedsResourceInfo : ConditionalTraitInfo
    {
        public readonly int Interval = 75;
        public readonly string ResourceType = "Ore";//默认矿石
        public readonly int MaxRange = 100;


        public override object Create(ActorInitializer init)
        {
            return new SeedsResource(init.Self, this);
        }
    }
    class SeedsResource:ConditionalTrait<SeedsResourceInfo>,ITick
    {
        readonly SeedsResourceInfo info;

        readonly ResourceType resourceType;
        readonly ResourceLayer resLayer;

        int ticks;

        public SeedsResource(Actor self,SeedsResourceInfo info) : base(info)
        {
            this.info = info;

            resourceType = self.World.WorldActor.TraitsImplementing<ResourceType>().FirstOrDefault(t => t.Info.Type == info.ResourceType);

            if (resourceType == null)
                throw new InvalidOperationException("No such resource type '{0}'".F(info.ResourceType));

            resLayer = self.World.WorldActor.Trait<ResourceLayer>();

        }


        public void Tick(Actor self)
        {
            if (IsTraitDisabled)
                return;

            if (--ticks <= 0)
            {
                Seed(self);
                ticks = info.Interval;
            }
        }

        public void Seed(Actor self)
        {
            var cell = Util.RandomWalk(self.Location, self.World.SharedRandom).
                Take(info.MaxRange).
                SkipWhile(p => !self.World.Map.Contains(p) || (resLayer.GetResource(p) == resourceType && resLayer.IsFull(p))).
                Cast<CPos?>().FirstOrDefault();

            if (cell != null && resLayer.CanSpawnResourceAt(resourceType, cell.Value))
                resLayer.AddResource(resourceType, cell.Value, 1);
        }



    }

    
}