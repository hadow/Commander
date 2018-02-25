using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class SupportPowerManagerInfo : ITraitInfo, Requires<TechTreeInfo>
    {
        public object Create(ActorInitializer init) { return new SupportPowerManager(init); }
    }
    public class SupportPowerManager:ITick,IResolveOrder,ITechTreeElement
    {
        public readonly Actor Self;
        public readonly Dictionary<string, SupportPowerInstance> Powers = new Dictionary<string, SupportPowerInstance>();

        public readonly TechTree TechTree;

        public readonly Lazy<RadarPings> RadarPings;

        public SupportPowerManager(ActorInitializer init)
        {
            Self = init.Self;

            TechTree = Self.Trait<TechTree>();
            RadarPings = Exts.Lazy(() => init.World.WorldActor.TraitOrDefault<RadarPings>());

            init.World.ActorAdded += ActorAdded;
            init.World.ActorRemoved += ActorRemoved;
        }

        void ITick.Tick(Actor self)
        {

        }

        void IResolveOrder.ResolveOrder(Actor self, NetWork.Order order)
        {

        }

        void ActorAdded(Actor a)
        {

        }

        void ActorRemoved(Actor a)
        {

        }

        public void PrerequisitesAvailable(string key)
        {

        }

        public void PrerequisitesUnavailable(string key)
        {

        }

        public void PrerequisitesItemHidden(string key) { }

        public void PrerequisitesItemVisible(string key) { }
    }


    public class SupportPowerInstance
    {

    }
}