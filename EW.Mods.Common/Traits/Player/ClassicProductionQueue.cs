using System;
using System.Linq;
using System.Collections.Generic;
using EW.Primitives;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class ClassicProductionQueueInfo : ProductionQueueInfo, Requires<TechTreeInfo>, Requires<PowerManagerInfo>, Requires<PlayerResourcesInfo>
    {
        [Desc("If you build more actors of the same type,", "the same queue will get its build time lowered for every actor produced there.")]
        public readonly bool SpeedUp = false;


        [Desc("Every time another production building of the same queue is",
            "constructed, the build times of all actors in the queue",
            "decreased by a percentage of the original time.")]
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
            // PERF: Avoid LINQ.
            Enabled = false;
            var isActive = false;
            foreach (var x in self.World.ActorsWithTrait<Production>())
            {
                if (x.Trait.IsTraitDisabled)
                    continue;

                if (x.Actor.Owner != self.Owner || !x.Trait.Info.Produces.Contains(Info.Type))
                    continue;

                Enabled |= IsValidFaction;
                isActive |= !x.Trait.IsTraitPaused;
            }

            if (!Enabled)
                ClearQueue();

            TickInner(self, !isActive);
        }

        public override IEnumerable<ActorInfo> AllItems()
        {
            return Enabled ? base.AllItems() : NoItems;
        }

        public override IEnumerable<ActorInfo> BuildableItems()
        {
            return Enabled ? base.BuildableItems() : NoItems;
        }

        public override TraitPair<Production> MostLikelyProducer()
        {
            var productionActors = self.World.ActorsWithTrait<Production>()
                .Where(x => x.Actor.Owner == self.Owner
                    && !x.Trait.IsTraitDisabled && x.Trait.Info.Produces.Contains(Info.Type))
                .OrderByDescending(x => x.Actor.IsPrimaryBuilding())
                .ThenByDescending(x => x.Actor.ActorID)
                .ToList();

            var unpaused = productionActors.FirstOrDefault(a => !a.Trait.IsTraitPaused);
            return unpaused.Trait != null ? unpaused : productionActors.FirstOrDefault();
        }



        protected override bool BuildUnit(ActorInfo unit)
        {

            // Find a production structure to build this actor
            var bi = unit.TraitInfo<BuildableInfo>();

            // Some units may request a specific production type, which is ignored if the AllTech cheat is enabled
            var type = developerMode.AllTech ? Info.Type : (bi.BuildAtProductionType ?? Info.Type);

            var producers = self.World.ActorsWithTrait<Production>()
                .Where(x => x.Actor.Owner == self.Owner
                    && !x.Trait.IsTraitDisabled
                    && x.Trait.Info.Produces.Contains(type))
                    .OrderByDescending(x => x.Actor.IsPrimaryBuilding())
                    .ThenByDescending(x => x.Actor.ActorID);


            if (!producers.Any())
            {
                CancelProduction(unit.Name, 1);
                return false;
            }

            foreach (var p in producers)
            {
                if (p.Trait.IsTraitPaused)
                    continue;

                var inits = new TypeDictionary
                {
                    new OwnerInit(self.Owner),
                    new FactionInit(BuildableInfo.GetInitialFaction(unit, p.Trait.Faction))
                };

                if (p.Trait.Produce(p.Actor, unit, type, inits))
                {
                    FinishProduction();
                    return true;
                }
            }

            return false;
        }


        public override int GetBuildTime(ActorInfo unit, BuildableInfo bi)
        {
            if (developerMode.FastBuild)
                return 0;

            var time = base.GetBuildTime(unit, bi);

            if (info.SpeedUp)
            {
                var type = bi.BuildAtProductionType ?? info.Type;

                var selfsameProductionsCount = self.World.ActorsWithTrait<Production>()
                    .Count(p => !p.Trait.IsTraitDisabled && !p.Trait.IsTraitPaused && p.Actor.Owner == self.Owner && p.Trait.Info.Produces.Contains(type));

                var speedModifier = selfsameProductionsCount.Clamp(1, info.BuildTimeSpeedReduction.Length) - 1;
                time = (time * info.BuildTimeSpeedReduction[speedModifier]) / 100;
            }

            return time;
        }

    }
}