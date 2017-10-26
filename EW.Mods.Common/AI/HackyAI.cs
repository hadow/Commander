using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Support;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.AI
{


    public sealed class HackyAIInfo : ITraitInfo
    {

        /// <summary>
        /// 
        /// internal id for this bot
        /// </summary>
        /// 
        [FieldLoader.Require]
        public readonly string Type = null;

        public readonly string Name = "Unnamed Bot";

        /// <summary>
        /// Minimum number of  units AI must have before attacking.
        /// </summary>
        public readonly int SquadSize = 8;












        public object Create(ActorInitializer init) { return new HackyAI(this, init); }
    }

    public sealed class HackyAI:ITick,INotifyDamage
    {
        public MersenneTwister Random { get; private set; }

        public readonly World World;

        public Map Map { get { return World.Map; } }

        public readonly HackyAIInfo Info;

        readonly Func<Actor, bool> isEnemyUnit;

        readonly Predicate<Actor> unitCannotBeOrdered;
        
        public List<Squad> Squads = new List<Squad>();

        readonly DomainIndex domainIndex;

        public bool IsEnabled;

        CPos initialBaseCenter;

        PowerManager playerPower;

        PlayerResources playerResource;

        /// <summary>
        /// 
        /// </summary>
        List<Actor> activeUnits = new List<Actor>();

        public Player Player { get; private set; }

        readonly Queue<Order> orders = new Queue<Order>();

        public HackyAI(HackyAIInfo info,ActorInitializer init)
        {
            Info = info;
            World = init.World;

            domainIndex = World.WorldActor.Trait<DomainIndex>();

            isEnemyUnit = unit =>
                Player.Stances[unit.Owner] == Stance.Enemy
                && !unit.Info.HasTraitInfo<HuskInfo>()
                && unit.Info.HasTraitInfo<ITargetableInfo>();
        }

        public void Tick(Actor self)
        {

        }

        internal Actor FindClosestEnemy(WPos pos)
        {
            return World.Actors.Where(isEnemyUnit).ClosestTo(pos);
        }
    }
}