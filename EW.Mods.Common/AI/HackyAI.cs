using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Support;
using EW.Mods.Common.Traits;
using EW.Mods.Common.Pathfinder;
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

            }

            if(--minAttackForceDelayTicks<=0){

                minAttackForceDelayTicks = Info.MinimumAttackForceDelay;
                CreateAttackForce();

            }


            if(--minCaptureDelayTicks<=0){
                minCaptureDelayTicks = Info.MinimumCaptureDelay;

            }




        }

        /// <summary>
        /// Crates the attack force.
        /// </summary>

        void CreateAttackForce(){

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


        }

       

        CPos FindNextResource(Actor actor,Harvester harv){

            var mobileInfo = actor.Info.TraitInfo<MobileInfo>();
            var passable = (uint)mobileInfo.GetMovementClass(World.Map.Rules.TileSet);

            Func<CPos, bool> isValidResource;
            var path = pathfinder.FindPath(
                PathSearch.Search(World, mobileInfo, actor, true, isValidResource)
                .WithCustomCost(loc => World.FindActorsInCircle(World.Map.CenterOfCell(loc), Info.HarvesterEnemyAvoidanceRadius)
                                .Where(u => !u.IsDead && actor.Owner.Stances[u.Owner] == Stance.Enemy)
                                .Sum(u => Math.Max(WDist.Zero.Length, Info.HarvesterEnemyAvoidanceRadius.Length - (World.Map.CenterOfCell(loc) - u.CenterPosition).Length))).FromPoint(actor.Location));

            if (path.Count == 0)
                return CPos.Zero;
            return path[0];
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

            if (!IsEnabled || e.attacker == null)
                return;

            if (e.attacker.Owner.Stances[self.Owner] == Stance.Neutral)
                return;

            var rb = self.TraitOrDefault<RepairableBuilding>();

            if(Info.ShouldRepairBuildings && rb != null){


            }

            if (e.attacker.Disposed)
                return;

            if (!e.attacker.Info.HasTraitInfo<ITargetableInfo>())
                return;

            if((self.Info.HasTraitInfo<HarvesterInfo>() ||
                self.Info.HasTraitInfo<BuildingInfo>() || self.Info.HasTraitInfo<BaseBuildingInfo>())
               && Player.Stances[e.attacker.Owner] == Stance.Enemy)
            {
                defenseCenter = e.attacker.Location;

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