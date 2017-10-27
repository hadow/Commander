using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Support;
using EW.Mods.Common.Traits;
namespace EW.Mods.Common.AI
{


    public sealed class HackyAIInfo : ITraitInfo,IBotInfo
    {

        public class UnitCategories
        {
            public readonly HashSet<string> Mcv = new HashSet<string>();
            public readonly HashSet<string> NavalUnits = new HashSet<string>();//海军单位
            public readonly HashSet<string> ExcludeFromSquads = new HashSet<string>();
        }
        public class BuildingCategories
        {
            public readonly HashSet<string> ConstructionYard = new HashSet<string>();
            public readonly HashSet<string> VehiclesFactory = new HashSet<string>();
            public readonly HashSet<string> Refinery = new HashSet<string>();
            public readonly HashSet<string> Power = new HashSet<string>();
            public readonly HashSet<string> Barracks = new HashSet<string>();//兵营
            public readonly HashSet<string> Production = new HashSet<string>();
            public readonly HashSet<string> Silo = new HashSet<string>();//仓

        }
        /// <summary>
        /// 
        /// internal id for this bot
        /// </summary>
        /// 
        [FieldLoader.Require]
        public readonly string Type = null;

        public readonly string Name = "Unnamed Bot";


        string IBotInfo.Type { get { return Type; } }

        string IBotInfo.Name { get { return Name; } }

        /// <summary>
        /// Minimum number of  units AI must have before attacking.
        /// </summary>
        public readonly int SquadSize = 8;


        /// <summary>
        /// Random number of up to this many units is added to squad size when creating an attack squad.
        /// </summary>
        public readonly int SquadSizeRandomBonus = 30;


        /// <summary>
        /// Production queues AI uses for buildings.
        /// </summary>
        public readonly HashSet<string> BuildingQueues = new HashSet<string> { "Building" };

        /// <summary>
        /// Production queues AI uses for defenses.
        /// </summary>
        public readonly HashSet<string> DefenseQueues = new HashSet<string> { "Defense" };

        /// <summary>
        /// Delay (int ticks) between giving out orders to units.
        /// </summary>
        public readonly int AssignRolesInterval = 20;

        /// <summary>
        /// Delay (int ticks) between attempting rush attacks.
        /// </summary>
        public readonly int RushInterval = 600;

        /// <summary>
        /// Delay (int ticks) between updating squads.
        /// </summary>
        public readonly int AttackForceInterval = 30;
        
        /// <summary>
        /// Radius in cells around the base that should be scanned for units to be protected.
        /// </summary>
        public readonly int ProtectUnitScanRadius = 15;

        /// <summary>
        /// Radius in cells around enemy BaseBuilder (Construction Yard) where AI scans for targets to rush.
        /// </summary>
        public readonly int RushAttackScanRadius = 15;

        /// <summary>
        /// 
        /// </summary>
        public readonly HashSet<string> UnitQueues = new HashSet<string> { "Vehicle", "Infantry", "Plane", "Ship", "Aircraft" };
        /// <summary>
        /// Should the AI repair its buildings if damaged?
        /// </summary>
        public readonly bool ShouldRepairBuildings = true;
        /// <summary>
        /// 
        /// </summary>
        public readonly int MinimumAttackForceDelay = 0;

        public readonly int StructureProductionInactiveDelay = 125;


        [FieldLoader.LoadUsing("LoadUnitCategories",true)]
        public readonly UnitCategories UnitsCommonNames;

        [FieldLoader.LoadUsing("LoadBuildingCategories",true)]
        public readonly BuildingCategories BuildingCommonNames;

        public object Create(ActorInitializer init) { return new HackyAI(this, init); }


        static object LoadUnitCategories(MiniYaml yaml)
        {
            var categories = yaml.Nodes.First(n => n.Key == "UnitsCommonNames");
            return FieldLoader.Load<UnitCategories>(categories.Value);
        }

        static object LoadBuildingCategories(MiniYaml yaml)
        {
            var categories = yaml.Nodes.First(n => n.Key == "BuildingCommonNames");
            return FieldLoader.Load<BuildingCategories>(categories.Value);
        }
    }

    public sealed class HackyAI:ITick,INotifyDamage,IBot
    {
        public MersenneTwister Random { get; private set; }

        public readonly World World;

        public Map Map { get { return World.Map; } }

        IBotInfo IBot.Info { get { return Info; } }

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
            if (!IsEnabled)
                return;

            
        }

        internal Actor FindClosestEnemy(WPos pos)
        {
            return World.Actors.Where(isEnemyUnit).ClosestTo(pos);
        }

        public void Activate(Player p)
        {

        }
    }
}