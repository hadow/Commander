using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Support;
using EW.NetWork;
using EW.Mods.Common.Traits;
using EW.Mods.Common.Pathfinder;
using EW.Mods.Common.Activities;
namespace EW.Mods.Common.AI
{

    public enum BuildingType { Building, Defense, Refinery }

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

            public readonly HashSet<string> NavalProduction = new HashSet<string>();//海军生产


        }
        /// <summary>
        /// 
        /// internal id for this bot
        /// </summary>
        /// 
        //[FieldLoader.Require]
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
        /// 在创建攻击队列时，
        /// </summary>
        public readonly int SquadSizeRandomBonus = 30;


        /// <summary>
        /// Production queues AI uses for buildings.
        /// </summary>
        public readonly HashSet<string> BuildingQueues = new HashSet<string> { "Building" };

        /// <summary>
        /// Production queues AI uses for defenses.
        /// 
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
        /// Radius in cells around  a factory scanned for rally points by the AI
        /// 集结点
        /// </summary>
        public readonly int RallyPointScanRadius = 8;

        /// <summary>
        /// Radius in cells that squads should scan for danger around their position to make flee decisions.
        /// </summary>
        public readonly int DangerScanRadius = 10;

        /// <summary>
        /// Radius in cells that squads should scan for enemies around their position when trying to attack.
        /// </summary>
        public readonly int AttackScanRadius = 12;


        /// <summary>
        /// Radius in cells that squads should scan for enemies around their position while idle.
        /// </summary>
        public readonly int IdleScanRadius = 10;

        /// <summary>
        /// Avoid enemy actors nearby when searching for a new resource path.Should be somewhere near the max weapon range.
        /// </summary>
        public readonly WDist HarvesterEnemyAvoidanceRadius = WDist.FromCells(8);

        /// <summary>
        /// 
        /// </summary>
        public readonly HashSet<string> UnitQueues = new HashSet<string> { "Vehicle", "Infantry", "Plane", "Ship", "Aircraft" };
        /// <summary>
        /// Should the AI repair its buildings if damaged?
        /// </summary>
        public readonly bool ShouldRepairBuildings = true;
        /// <summary>
        /// Minimum delay (in ticks) between creating squads.
        /// </summary>
        public readonly int MinimumAttackForceDelay = 0;


        public readonly int MinOrderQuotienPerTick = 5;


        public readonly int MinimumCaptureDelay = 375;

        /// <summary>
        /// Minimum excess power the AI should try to maintain.
        /// </summary>
        public readonly int MinimumExcessPower = 0;



        public readonly int StructureProductionInactiveDelay = 125;

        /// <summary>
        /// Delay (in ticks) until retrying to build structure after the last 3 consecutive attempts failed.
        /// </summary>
        public readonly int StructureProductionResumeDelay = 1500;

        /// <summary>
        /// What units to the AI Should  build."What % of the total army must be this type of unit.
        /// </summary>
        public readonly Dictionary<string, float> UnitsToBuild = null;

        /// <summary>
        /// What units shuld the Ai have a maximum limit to train.
        /// </summary>
        public readonly Dictionary<string, int> UnitLimits = null;

        /// <summary>
        /// what buildings to the AI should build." what % of  the total base must be this type of building"
        /// </summary>
        public readonly Dictionary<string, float> BuildingFractions = null;

        /// <summary>
        /// What buildings should the AI have a maximum limit to build.
        /// </summary>
        public readonly Dictionary<string, int> BuildingLimits = null;

        [FieldLoader.LoadUsing("LoadUnitCategories",true)]
        public readonly UnitCategories UnitsCommonNames;

        [FieldLoader.LoadUsing("LoadBuildingCategories",true)]
        public readonly BuildingCategories BuildingCommonNames;

        [Desc("After how many failed attempts to place a structure should AI give up and wait",
            "for StructureProductionResumeDelay before retrying.")]
        public readonly int MaximumFailedPlacementAttempts = 3;

        [Desc("Try to build another production building if there is too much cash.")]
        public readonly int NewProductionCashThreshold = 5000;


        [Desc("Radius in cells around each building with ProvideBuildableArea",
            "to check for a 3x3 area of water where naval structures can be built.",
            "Should match maximum adjacency of naval structures.")]
        public readonly int CheckForWaterRadius = 8;


        [Desc("Terrain types which are considered water for base building purposes.")]
        public readonly HashSet<string> WaterTerrainTypes = new HashSet<string> { "Water" };


        [Desc("Minimum range at which to build defensive structures near a combat hotspot.")]
        public readonly int MinimumDefenseRadius = 5;

        [Desc("Maximum range at which to build defensive structures near a combat hotspot.")]
        public readonly int MaximumDefenseRadius = 20;

        [Desc("Minimum distance in cells from center of the base when checking for building placement.")]
        public readonly int MinBaseRadius = 2;

        [Desc("Radius in cells around the center of the base to expand.")]
        public readonly int MaxBaseRadius = 20;

        [Desc("How many randomly chosen cells with resources to check when deciding refinery placement.")]
        public readonly int MaxResourceCellsToCheck = 3;


        [Desc("Delay (in ticks) until rechecking for new BaseProviders.")]
        public readonly int CheckForNewBasesDelay = 1500;

        [Desc("A random delay (in ticks) of up to this is added to active/inactive production delays.")]
        public readonly int StructureProductionRandomBonusDelay = 10;

        [Desc("Additional delay (in ticks) added between structure production checks when actively building things.",
            "Note: The total delay is gamespeed OrderLatency x 4 + this + StructureProductionRandomBonusDelay.")]
        public readonly int StructureProductionActiveDelay = 0;


        [Desc("Should deployment of additional MCVs be restricted to MaxBaseRadius if explicit deploy locations are missing or occupied?")]
        public readonly bool RestrictMCVDeploymentFallbackToBase = true;

        [Desc("Only produce units as long as there are less than this amount of units idling inside the base.")]
        public readonly int IdleBaseUnitsMaximum = 12;

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

        readonly ResourceLayer resLayer;
        readonly ResourceClaimLayer claimLayer;
        readonly IPathFinder pathfinder;




        public bool IsEnabled;

        CPos initialBaseCenter;

        CPos defenseCenter;

        PowerManager playerPower;

        SupportPowerManager supportPowerMngr;

        FrozenActorLayer froenLayer;

        int ticks;

        BitArray resourceTypeIndices;


        PlayerResources playerResource;


        List<BaseBuilder> builders = new List<BaseBuilder>();
        /// <summary>
        /// Units that the ai already knows about.Any unit not on this list needs to be given a role.
        /// </summary>
        List<Actor> activeUnits = new List<Actor>();


        List<Actor> unitsHangingAroundTheBase = new List<Actor>();

        public const int FeedbackTime = 30; // ticks;= a bit over 1s,

        public Player Player { get; private set; }

        readonly Queue<Order> orders = new Queue<Order>();

        int rushTicks;
        int assignRolesTicks;
        int attackForceTicks;
        int minAttackForceDelayTicks;
        int minCaptureDelayTicks;

        public HackyAI(HackyAIInfo info,ActorInitializer init)
        {
            Info = info;
            World = init.World;

            domainIndex = World.WorldActor.Trait<DomainIndex>();
            resLayer = World.WorldActor.TraitOrDefault<ResourceLayer>();
            claimLayer = World.WorldActor.TraitOrDefault<ResourceClaimLayer>();
            pathfinder = World.WorldActor.Trait<IPathFinder>();

            isEnemyUnit = unit =>
                Player.Stances[unit.Owner] == Stance.Enemy
                && !unit.Info.HasTraitInfo<HuskInfo>()
                && unit.Info.HasTraitInfo<ITargetableInfo>();

            unitCannotBeOrdered = a => a.Owner != Player || a.IsDead || !a.IsInWorld;
        }


        public CPos GetRandomBaseCenter(){

            var randomConstructionYard = World.Actors.Where(a => a.Owner == Player 
                                                            && Info.BuildingCommonNames.ConstructionYard.Contains(a.Info.Name)).RandomOrDefault(Random);

            return randomConstructionYard != null ? randomConstructionYard.Location : initialBaseCenter;

        }

        public void Activate(Player p){

            Player = p;
            IsEnabled = true;
            playerPower = p.PlayerActor.Trait<PowerManager>();
            supportPowerMngr = p.PlayerActor.Trait<SupportPowerManager>();
            playerResource = p.PlayerActor.Trait<PlayerResources>();
            froenLayer = p.PlayerActor.Trait<FrozenActorLayer>();


            foreach(var building in Info.BuildingQueues){
                builders.Add(new BaseBuilder(this,building,p,playerPower,playerResource));
            }

            foreach(var defense in Info.DefenseQueues){
                builders.Add(new BaseBuilder(this,defense,p,playerPower,playerResource));
            }

            Random = new MersenneTwister(WarGame.CosmeticRandom.Next());

            //Avoid all AIs trying to rush in the same tick, randomize their initial rush a little.
            var smallFractionOfRushInterval = Info.RushInterval / 20;
            rushTicks = Random.Next(Info.RushInterval - smallFractionOfRushInterval, Info.RushInterval + smallFractionOfRushInterval);

            //Avoid all AIs reevaluating assignments on the same tick, randomize their initial evaluation delay.
            assignRolesTicks = Random.Next(0, Info.AssignRolesInterval);
            attackForceTicks = Random.Next(0, Info.AttackForceInterval);
            minAttackForceDelayTicks = Random.Next(0, Info.MinimumAttackForceDelay);
            minCaptureDelayTicks = Random.Next(0, Info.MinimumCaptureDelay);

            var tileset = World.Map.Rules.TileSet;
            resourceTypeIndices = new BitArray(tileset.TerrainInfo.Length);//Big enough

            foreach (var t in Map.Rules.Actors["world"].TraitInfos<ResourceTypeInfo>())
                resourceTypeIndices.Set(tileset.GetTerrainIndex(t.TerrainType), true);


        }

        void ITick.Tick(Actor self)
        {
            if (!IsEnabled)
                return;

            ticks++;
            if (ticks == 1)
                InitializeBase(self);

            if (ticks % FeedbackTime == 0)
                ProductionUnits(self);

            AssignRolesToIdleUnits(self);
            SetRallyPointsForNewProductionBuildings(self);

            foreach (var b in builders)
                b.Tick();

            var ordersToIssueThisTick = Math.Min((orders.Count + Info.MinOrderQuotienPerTick - 1) / Info.MinOrderQuotienPerTick, orders.Count);

            for (var i = 0; i < ordersToIssueThisTick; i++)
                World.IssueOrder(orders.Dequeue());
            
        }

        internal IEnumerable<ProductionQueue> FindQueues(string category){

            return World.ActorsWithTrait<ProductionQueue>()
                .Where(a => a.Actor.Owner == Player && a.Trait.Info.Type == category && a.Trait.Enabled)
                .Select(a => a.Trait);
        }


        public bool EnoughWaterToBuildNaval(){

            var baseProviders = World.ActorsHavingTrait<BaseProvider>()
                .Where(a => a.Owner == Player);

            foreach(var b in baseProviders){

                var playerWorld = Player.World;
                var countWaterCells = Map.FindTilesInCircle(b.Location, Info.MaxBaseRadius)
                                         .Where(c => playerWorld.Map.Contains(c)
                                                && Info.WaterTerrainTypes.Contains(playerWorld.Map.GetTerrainInfo(c).Type)
                                                && Util.AdjacentCells(playerWorld, Target.FromCell(playerWorld, c))
                                                .All(a => Info.WaterTerrainTypes.Contains(playerWorld.Map.GetTerrainInfo(a).Type))).Count();

                if (countWaterCells > 0)
                    return true;

            }

            return false;
        }

        // Check whether we have at least one building providing buildable area close enough to water to build naval structures
        public bool CloseEnoughToWater(){

            var areaProviders = World.ActorsHavingTrait<GivesBuildableArea>().Where(a => a.Owner == Player);

            foreach (var a in areaProviders){

                var playerWorld = Player.World;
                var adjacentWater = Map.FindTilesInCircle(a.Location, Info.CheckForWaterRadius)
                                       .Where(c => playerWorld.Map.Contains(c)
                                              && Info.WaterTerrainTypes.Contains(playerWorld.Map.GetTerrainInfo(c).Type)
                                              && Util.AdjacentCells(playerWorld, Target.FromCell(playerWorld, c))
                                              .All(ac => Info.WaterTerrainTypes.Contains(playerWorld.Map.GetTerrainInfo(ac).Type))).Count();

                if (adjacentWater > 0)
                    return true;
            }

            return false;
        }

        public CPos? ChooseBuildLocation(string actorType,bool distanceToBaseIsImportant,BuildingType type){

            var bi = Map.Rules.Actors[actorType].TraitInfoOrDefault<BuildingInfo>();
            if (bi == null)
                return null;

            // Find the buildable cell that is closest to pos and centered around center
            Func<CPos, CPos, int, int, CPos?> findPos = (center, target, minRange, maxRange) =>
            {
                var cells = Map.FindTilesInAnnulus(center, minRange, maxRange);

                // Sort by distance to target if we have one
                if (center != target)
                    cells = cells.OrderBy(c => (c - target).LengthSquard);
                else
                    cells = cells.Shuffle(Random);

                foreach(var cell in cells){

                    if (!World.CanPlaceBuilding(actorType, bi, cell, null))
                        continue;

                    if (distanceToBaseIsImportant && !bi.IsCloseEnoughToBase(World, Player, actorType, cell))
                        continue;

                    return cell;
                }
                return null;
            };

            var baseCenter = GetRandomBaseCenter();

            switch(type){

                case BuildingType.Defense:
                    // Build near the closest enemy structure
                    var closestEnemy = World.ActorsHavingTrait<Building>()
                        .Where(a => !a.Disposed && Player.Stances[a.Owner] == Stance.Enemy).ClosestTo(World.Map.CenterOfCell(defenseCenter));

                    var targetCell = closestEnemy != null ? closestEnemy.Location : baseCenter;
                    return findPos(defenseCenter, targetCell, Info.MinimumDefenseRadius, Info.MaximumDefenseRadius);
                case BuildingType.Refinery:

                    // Try and place the refinery near a resource field
                    var nearbyResources = Map.FindTilesInAnnulus(baseCenter, Info.MinBaseRadius, Info.MaxBaseRadius)
                        .Where(a => resourceTypeIndices.Get(Map.GetTerrainIndex(a)))
                        .Shuffle(Random).Take(Info.MaxResourceCellsToCheck);

                    foreach(var r in nearbyResources){

                        var found = findPos(baseCenter, r, Info.MinBaseRadius, Info.MaxBaseRadius);
                        if (found != null)
                            return found;
                        
                    }

                    // Try and find a free spot somewhere else in the base
                    return findPos(baseCenter, baseCenter, Info.MinBaseRadius, Info.MaxBaseRadius);

                case BuildingType.Building:
                    return findPos(baseCenter, baseCenter, Info.MinBaseRadius, distanceToBaseIsImportant ? Info.MaxBaseRadius : Map.Grid.MaximumTileSearchRange);

            }

            // Can't find a build location
            return null;

        }

        void SetRallyPointsForNewProductionBuildings(Actor self){

            foreach(var rp in self.World.ActorsWithTrait<RallyPoint>()){
                if(rp.Actor.Owner == Player && !IsRallyPointValid(rp.Trait.Location,rp.Actor.Info.TraitInfoOrDefault<BuildingInfo>()) )
                {
                    QueueOrder(new Order("SetRallyPoin", rp.Actor, Target.FromCell(World, ChooseRallyLocationNear(rp.Actor)), false)
                    {

                        SuppressVisualFeedback = true,
                    });
                }
            }
        }

        /// <summary>
        /// Chooses the rally location near.
        /// </summary>
        /// <returns>The rally location near.</returns>
        /// <param name="producer">Producer.</param>

        CPos ChooseRallyLocationNear(Actor producer){

            var possibleRallyPoints = Map.FindTilesInCircle(producer.Location, Info.RallyPointScanRadius)
                                         .Where(c => IsRallyPointValid(c, producer.Info.TraitInfoOrDefault<BuildingInfo>()));

            if(!possibleRallyPoints.Any()){
                return producer.Location;
            }

            return possibleRallyPoints.Random(Random);

        }

        bool IsRallyPointValid(CPos x,BuildingInfo info){
            return info != null && World.IsCellBuildable(x, info);
        }


        /// <summary>
        /// Assigns the roles to idle units.
        /// </summary>
        /// <param name="self">Self.</param>
        void AssignRolesToIdleUnits(Actor self){

            CleanSquads();

            activeUnits.RemoveAll(unitCannotBeOrdered);
            unitsHangingAroundTheBase.RemoveAll(unitCannotBeOrdered);

            if(--rushTicks<=0){
                rushTicks = Info.RushInterval;
                TryToRushAttack();
            }

            if(--attackForceTicks<=0){
                attackForceTicks = Info.AttackForceInterval;
                foreach (var s in Squads)
                    s.Update();
            }

            if(--assignRolesTicks<=0){

                assignRolesTicks = Info.AssignRolesInterval;
                if (resLayer != null && !resLayer.IsResourceLayerEmpty)
                    GiveOrdersToIdleHarvesters();
                
                FindNewUnits(self);
                FindAndDeployBackupMcv(self);

            }

            if(--minAttackForceDelayTicks<=0){

                minAttackForceDelayTicks = Info.MinimumAttackForceDelay;
                CreateAttackForce();

            }


            if(--minCaptureDelayTicks<=0){
                minCaptureDelayTicks = Info.MinimumCaptureDelay;

            }




        }


        void GiveOrdersToIdleHarvesters(){


            // Find idle harvesters and give them orders:
            foreach(var harvester in activeUnits){

                var harv = harvester.TraitOrDefault<Harvester>();
                if (harv == null)
                    continue;

                if (!harv.IsEmpty)
                    continue;

                if(!harvester.IsIdle)
                {
                    var act = harvester.CurrentActivity;
                    if (!harv.LastSearchFailed || act.NextActivity == null || act.NextActivity.GetType() != typeof(FindResources))
                        continue;
                    
                }

                var para = harvester.TraitOrDefault<Parachutable>();
                if (para != null && para.IsInAir)
                    continue;

                // Tell the idle harvester to quit slacking:
                var newSafeResourcePatch = FindNextResource(harvester, harv);

                QueueOrder(new Order("Harvest",harvester,Target.FromCell(World,newSafeResourcePatch),false));

            }
        }

        /// <summary>
        /// Crates the attack force.
        /// </summary>

        void CreateAttackForce(){

            // Create an attack force when we have enough units around our base.
            // (don't bother leaving any behind for defense)
            var randomizedSquadSize = Info.SquadSize + Random.Next(Info.SquadSizeRandomBonus);

            if(unitsHangingAroundTheBase.Count >= randomizedSquadSize){
                var attackForce = RegisterNewSquad(SquadT.Assault);

                foreach(var a in unitsHangingAroundTheBase){
                    if (!a.Info.HasTraitInfo<AircraftInfo>())
                        attackForce.Units.Add(a);
                }

                unitsHangingAroundTheBase.Clear();
            }

        }


        Squad RegisterNewSquad(SquadT type,Actor target =null){

            var ret = new Squad(this, type, target);
            Squads.Add(ret);
            return ret;
        }


        void TryToRushAttack(){

            var allEnemyBaseBuilder = FindEnemyConstructionYards();
            var ownUnits = activeUnits.Where(unit=>unit.IsIdle
                                             &&unit.Info.HasTraitInfo<AttackBaseInfo>() 
                                             && !unit.Info.HasTraitInfo<AircraftInfo>() 
                                             && !unit.Info.HasTraitInfo<HarvesterInfo>()).ToList();

            if (!allEnemyBaseBuilder.Any() || (ownUnits.Count < Info.SquadSize))
                return;

            foreach(var  b in allEnemyBaseBuilder){

                var enemies = World.FindActorsInCircle(b.CenterPosition, WDist.FromCells(Info.RushAttackScanRadius))
                                   .Where(unit => Player.Stances[unit.Owner] == Stance.Enemy && unit.Info.HasTraitInfo<AttackBaseInfo>()).ToList();

                if(AttackOrFleeFuzzy.Rush.CanAttack(ownUnits,enemies)){

                    var target = enemies.Any() ? enemies.Random(Random) : b;
                    var rush = GetSquadOfType(SquadT.Rush);
                    if (rush == null)
                        rush = RegisterNewSquad(SquadT.Rush, target);
                    foreach (var a3 in ownUnits)
                        rush.Units.Add(a3);

                    return;
                }

            }

        }

        Squad GetSquadOfType(SquadT type){

            return Squads.FirstOrDefault(s => s.Type == type);
        }



        void CleanSquads(){

            Squads.RemoveAll(s=>!s.IsValid);

            foreach(var s in Squads){
                s.Units.RemoveAll(unitCannotBeOrdered);
            }
        }

        /// <summary>
        /// Initializes the base.
        /// </summary>
        /// <param name="self">Self.</param>
        void InitializeBase(Actor self){

            //Find and deploy our mcv
            var mcv = self.World.Actors.FirstOrDefault(a => a.Owner == Player && Info.UnitsCommonNames.Mcv.Contains(a.Info.Name));

            if(mcv != null){

                initialBaseCenter = mcv.Location;
                defenseCenter = mcv.Location;
                QueueOrder(new Order("DeployTransform",mcv,false));
            }
            else{


            }
        }

        void ProductionUnits(Actor self){

            // Stop building until economy is restored
            if (!HasAdequateProc())
                return;

            // No construction yards - Build a new MCV
            if (Info.UnitsCommonNames.Mcv.Any() && !HasAdequateFact() && !self.World.Actors.Any(a => a.Owner == Player && Info.UnitsCommonNames.Mcv.Contains(a.Info.Name)))
                BuildUnit("Vehicle", GetInfoByCommonName(Info.UnitsCommonNames.Mcv, Player).Name);

            foreach (var q in Info.UnitQueues)
                BuildUnit(q, unitsHangingAroundTheBase.Count < Info.IdleBaseUnitsMaximum);
                
        }

        public ActorInfo GetInfoByCommonName(HashSet<string> names,Player owner){

            return Map.Rules.Actors.Where(k => names.Contains(k.Key)).Random(Random).Value;
        }

        ActorInfo ChooseRandomUnitToBuild(ProductionQueue queue){

            var buildableThings = queue.BuildableItems();
            if (!buildableThings.Any())
                return null;

            var unit = buildableThings.Random(Random);
            return HasAdequateAirUnitReloadBuildings(unit) ? unit : null;

        }

        ActorInfo ChooseUnitToBuild(ProductionQueue queue){

            var buildableThings = queue.BuildableItems();
            if (!buildableThings.Any())
                return null;

            var myUnits = Player.World.ActorsHavingTrait<IPositionable>()
                                .Where(a => a.Owner == Player).Select(a => a.Info.Name).ToList();

            foreach (var unit in Info.UnitsToBuild.Shuffle(Random))
            {
                if (buildableThings.Any(b => b.Name == unit.Key))
                    if (myUnits.Count(a => a == unit.Key) < unit.Value * myUnits.Count)
                        if (HasAdequateAirUnitReloadBuildings(Map.Rules.Actors[unit.Key]))
                            return Map.Rules.Actors[unit.Key];
            }
            return null;
        }


        // For mods like RA (number of building must match the number of aircraft)
        bool HasAdequateAirUnitReloadBuildings(ActorInfo actorInfo)
        {
            var aircraftInfo = actorInfo.TraitInfoOrDefault<AircraftInfo>();
            if (aircraftInfo == null)
                return true;

            // If the aircraft has at least 1 AmmoPool and defines 1 or more RearmBuildings, check if we have enough of those
            var hasAmmoPool = actorInfo.TraitInfos<AmmoPoolInfo>().Any();
            if (hasAmmoPool && aircraftInfo.RearmBuildings.Count > 0)
            {
                var countOwnAir = CountUnits(actorInfo.Name, Player);
                var countBuildings = aircraftInfo.RearmBuildings.Sum(b => CountBuilding(b, Player));
                if (countOwnAir >= countBuildings)
                    return false;
            }

            return true;
        }


        void BuildUnit(string category,string name){

            var queue = FindQueues(category).FirstOrDefault(q => q.CurrentItem() == null);
            if (queue == null)
                return;

            if (Map.Rules.Actors[name] != null)
                QueueOrder(Order.StartProduction(queue.Actor, name, 1));
        }

        void BuildUnit(string category,bool buildRandom){


            var queue = FindQueues(category).FirstOrDefault(q => q.CurrentItem() == null);
            if (queue == null)
                return;

            var unit = buildRandom ? ChooseRandomUnitToBuild(queue) : ChooseUnitToBuild(queue);

            if (unit == null)
                return;

            var name = unit.Name;

            if (Info.UnitsToBuild != null && !Info.UnitsToBuild.ContainsKey(name))
                return;

            if (Info.UnitLimits != null && Info.UnitLimits.ContainsKey(name) && World.Actors.Count(a => a.Owner == Player && a.Info.Name == name) >= Info.UnitLimits[name])
                return;

            QueueOrder(Order.StartProduction(queue.Actor,name,1));
        }

       

        CPos FindNextResource(Actor actor,Harvester harv){

            var mobileInfo = actor.Info.TraitInfo<MobileInfo>();
            var passable = (uint)mobileInfo.GetMovementClass(World.Map.Rules.TileSet);

            Func<CPos, bool> isValidResource = cell=>
                domainIndex.IsPassable(actor.Location,cell,mobileInfo,passable) &&
                harv.CanHarvestCell(actor,cell) &&
                claimLayer.CanClaimCell(actor,cell);
            var path = pathfinder.FindPath(
                PathSearch.Search(World, mobileInfo, actor, true, isValidResource)
                .WithCustomCost(loc => World.FindActorsInCircle(World.Map.CenterOfCell(loc), Info.HarvesterEnemyAvoidanceRadius)
                                .Where(u => !u.IsDead && actor.Owner.Stances[u.Owner] == Stance.Enemy)
                                .Sum(u => Math.Max(WDist.Zero.Length, Info.HarvesterEnemyAvoidanceRadius.Length - (World.Map.CenterOfCell(loc) - u.CenterPosition).Length))).FromPoint(actor.Location));

            if (path.Count == 0)
                return CPos.Zero;
            return path[0];
        }



        // Find any newly constructed MCVs and deploy them at a sensible
        // backup location.
        void FindAndDeployBackupMcv(Actor self){


            var mcvs = self.World.Actors.Where(a => a.Owner == Player && Info.UnitsCommonNames.Mcv.Contains(a.Info.Name));

            foreach(var mcv in mcvs){

                if (!mcv.IsIdle)
                    continue;

                var transformsInfo = mcv.Info.TraitInfoOrDefault<TransformsInfo>();
                if (transformsInfo == null)
                    continue;

                var restrictToBase = Info.RestrictMCVDeploymentFallbackToBase 
                                         && CountBuildingByCommonName(Info.BuildingCommonNames.ConstructionYard, Player) > 0;

                var desiredLocation = ChooseBuildLocation(transformsInfo.IntoActor, restrictToBase, BuildingType.Building);
                if (desiredLocation == null)
                    continue;

                QueueOrder(new Order("Move",mcv,Target.FromCell(World,desiredLocation.Value),true));
                QueueOrder(new Order("DeployTransform",mcv,true));
            }
        }


        //int CountBuildingByCommonName(HashSet<string> buildings,Player owner){

        //    return World.ActorsHavingTrait<Building>().Count(a => a.Owner == owner && buildings.Contains(a.Info.Name));
        //}

        void FindNewUnits(Actor self){


            var newUnits = self.World.ActorsHavingTrait<IPositionable>()
                               .Where(a => a.Owner == Player && !Info.UnitsCommonNames.Mcv.Contains(a.Info.Name) &&
                                      !Info.UnitsCommonNames.ExcludeFromSquads.Contains(a.Info.Name) && !activeUnits.Contains(a));

            foreach(var a in newUnits){


                if (a.Info.HasTraitInfo<HarvesterInfo>())
                    QueueOrder(new Order("Harvest", a, false));
                else
                    unitsHangingAroundTheBase.Add(a);

                if(a.Info.HasTraitInfo<AircraftInfo>() && a.Info.HasTraitInfo<AttackBaseInfo>())
                {
                    var air = GetSquadOfType(SquadT.Air);
                    if (air == null)
                        air = RegisterNewSquad(SquadT.Air);

                    air.Units.Add(a);
                }
                else if(Info.UnitsCommonNames.NavalUnits.Contains(a.Info.Name)){

                    var ships = GetSquadOfType(SquadT.Naval);
                    if (ships == null)
                        ships = RegisterNewSquad(SquadT.Naval);

                    ships.Units.Add(a);
                }

                activeUnits.Add(a);
            }

        }



        public bool HasAdequateFact(){

            // Require at least one construction yard, unless we have no vehicles factory (can't build it).
            return CountBuildingByCommonName(Info.BuildingCommonNames.ConstructionYard, Player) > 0 ||
                CountBuildingByCommonName(Info.BuildingCommonNames.VehiclesFactory, Player) == 0;
        }

        /// <summary>
        /// 
        /// 充足的
        /// </summary>
        /// <returns><c>true</c>, if adequate proc was hased, <c>false</c> otherwise.</returns>
        public bool HasAdequateProc(){

            return CountBuildingByCommonName(Info.BuildingCommonNames.Refinery, Player) > 0 ||
                CountBuildingByCommonName(Info.BuildingCommonNames.Power, Player) == 0 ||
                CountBuildingByCommonName(Info.BuildingCommonNames.ConstructionYard, Player) == 0;
        }

        public bool HasMinimumProc()
        {
            // Require at least two refineries, unless we have no power (can't build it)
            // or barracks (higher priority?)
            return CountBuildingByCommonName(Info.BuildingCommonNames.Refinery, Player) >= 2 ||
                CountBuildingByCommonName(Info.BuildingCommonNames.Power, Player) == 0 ||
                CountBuildingByCommonName(Info.BuildingCommonNames.Barracks, Player) == 0;
        }

        int CountBuildingByCommonName(HashSet<string> buildings,Player owner){

            return World.ActorsHavingTrait<Building>().Count(a => a.Owner == owner && buildings.Contains(a.Info.Name));
        }

        int CountBuilding(string frac,Player owner){
            return World.ActorsHavingTrait<Building>().Count(a => a.Owner == owner && a.Info.Name == frac);
        }

        int CountUnits(string unit,Player owner){
            return World.ActorsHavingTrait<IPositionable>().Count(a => a.Owner == owner && a.Info.Name == unit);
        }
        public void QueueOrder(Order order){

            orders.Enqueue(order);
        }


        internal Actor FindClosestEnemy(WPos pos)
        {
            return World.Actors.Where(isEnemyUnit).ClosestTo(pos);
        }

        internal Actor FindClosestEnemy(WPos pos,WDist radius){

            return World.FindActorsInCircle(pos, radius).Where(isEnemyUnit).ClosestTo(pos);
        }

        /// <summary>
        /// Finds the enemy construction yards.
        /// </summary>
        /// <returns>The enemy construction yards.</returns>
        List<Actor> FindEnemyConstructionYards()
        {

            return World.Actors.Where(a => Player.Stances[a.Owner] == Stance.Enemy && !a.IsDead &&
                                      Info.BuildingCommonNames.ConstructionYard.Contains(a.Info.Name)).ToList();

        }


        IEnumerable<Actor> GetVisibleActorsBelongingToPlayer(Player owner){

            foreach (var actor in GetActorsThatCanBeOrderedByPlayer(owner))
                if (actor.CanBeViewedByPlayer(owner))
                    yield return actor;
        }

        IEnumerable<Actor> GetActorsThatCanBeOrderedByPlayer(Player owner){

            foreach(var actor in World.Actors){
                if (actor.Owner == owner && !actor.IsDead && actor.IsInWorld)
                    yield return actor;
                
            }
        }
        public void Damaged(Actor self,AttackInfo e){

            if (!IsEnabled || e.Attacker == null)
                return;

            if (e.Attacker.Owner.Stances[self.Owner] == Stance.Neutral)
                return;

            var rb = self.TraitOrDefault<RepairableBuilding>();

            if(Info.ShouldRepairBuildings && rb != null){


            }

            if (e.Attacker.Disposed)
                return;

            if (!e.Attacker.Info.HasTraitInfo<ITargetableInfo>())
                return;

            if((self.Info.HasTraitInfo<HarvesterInfo>() ||
                self.Info.HasTraitInfo<BuildingInfo>() || self.Info.HasTraitInfo<BaseBuildingInfo>())
               && Player.Stances[e.Attacker.Owner] == Stance.Enemy)
            {
                defenseCenter = e.Attacker.Location;

            }
            
        }

        void ProtectOwn(Actor attacker){

            var protectSq = GetSquadOfType(SquadT.Protection);
            if (protectSq == null)
                protectSq = RegisterNewSquad(SquadT.Protection, attacker);

            if (!protectSq.IsTargetValid)
                protectSq.TargetActor = attacker;

            if(!protectSq.IsValid)
            {
                var ownUnits = World.FindActorsInCircle(World.Map.CenterOfCell(GetRandomBaseCenter()), WDist.FromCells(Info.ProtectUnitScanRadius))
                                    .Where(unit => unit.Owner == Player && !unit.Info.HasTraitInfo<BuildingInfo>() && !unit.Info.HasTraitInfo<HarvesterInfo>() && unit.Info.HasTraitInfo<AttackBaseInfo>());

                foreach (var a in ownUnits)
                    protectSq.Units.Add(a);
            }
                
        }
    }
}