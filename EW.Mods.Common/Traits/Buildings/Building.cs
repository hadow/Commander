using System;
using System.Collections.Generic;
using System.Linq;
using EW.Traits;
using EW.Primitives;
using EW.Graphics;

namespace EW.Mods.Common.Traits
{
    public enum FootprintCellType
    {
        Empty='_',              //measn completely empty.
        OccupiedPassable='=',   //means part of the footprint but passable.
        Occupied='x',         //means cell is blocked
        OccupiedUntargetable='X'   // means blocked but not counting as targetable  
    }

    public class BuildingInfo : ITraitInfo,IOccupySpaceInfo,IPlaceBuildingDecorationInfo,UsesInit<LocationInit>
    {

        /// <summary>
        /// Where you are allowed to place the building (water.clear....).
        /// </summary>
        public readonly HashSet<string> TerrainTypes = new HashSet<string>();


        public readonly CVec Dimensions = new CVec(1, 1);

        /// <summary>
        /// Shift center of the actor by this offset.
        /// 按此偏移量移动actor 中心
        /// </summary>
        public readonly WVec LocalCenterOffset = WVec.Zero;


        /// <summary>
        /// Clear smudges from underneath the building footprint.
        /// 清除建筑物足迹下面的污迹。
        /// </summary>
        public readonly bool RemoveSmudgesOnBuild = true;

        /// <summary>
        /// Clear smudges from underneath the building footprint on sell.
        /// </summary>
        public readonly bool RemoveSmudgesOnSell = true;

        /// <summary>
        /// Clear smudges from underneath the building footprint on transform.
        /// </summary>
        public readonly bool RemoveSmudgesOnTransform = true;

        public readonly bool RequiresBaseProvider = false;

        public readonly bool AllowInvalidPlacement = false;

        public readonly string[] BuildSounds = {"placbldg.aud", "build5.aud"  };

        public readonly string[] UndeploySounds = { "cashturn.aud"};//取消部署时播放的声音



        [FieldLoader.LoadUsing("LoadFootprint")]
        public readonly Dictionary<CVec, FootprintCellType> Footprint;

        protected static object LoadFootprint(MiniYaml yaml)
        {
            var footprintYaml = yaml.Nodes.FirstOrDefault(n => n.Key == "Footprint");

            var footprintChars = footprintYaml != null ? footprintYaml.Value.Value.Where(x => !Char.IsWhiteSpace(x)).ToArray() : new[] { 'x' };

            var dimensionsYaml = yaml.Nodes.FirstOrDefault(n => n.Key == "Dimensions");

            var dim = dimensionsYaml != null ? FieldLoader.GetValue<CVec>("Dimensions", dimensionsYaml.Value.Value) : new CVec(1, 1);


            if(footprintChars.Length != dim.X*dim.Y){
                var fp = footprintYaml.Value.Value;
                var dims = dim.X + "x" + dim.Y;
                throw new YamlException("Invalid footprint:{0} does not match dimensions {1}".F(fp, dims));
            }

            var index = 0;
            var ret = new Dictionary<CVec, FootprintCellType>();

            for (var y = 0; y < dim.Y;y++){

                for (var x = 0; x < dim.X;x++){

                    var c = footprintChars[index++];

                    if(!Enum.IsDefined(typeof(FootprintCellType),(FootprintCellType)c)){
                        throw new YamlException("Invalid footprint cell type '{0}'".F(c));
                    }

                    ret[new CVec(x, y)] = (FootprintCellType)c;
                }
            }

            return ret;
        }

        public IEnumerable<CPos> FootprintTiles(CPos location,FootprintCellType type)
        {
            return Footprint.Where(kv => kv.Value == type).Select(kv => location+kv.Key);
        }

        public IEnumerable<CPos> Tiles(CPos location)
        {
            foreach (var t in FootprintTiles(location, FootprintCellType.OccupiedPassable))
                yield return t;

            foreach (var t in FootprintTiles(location, FootprintCellType.Occupied))
                yield return t;

            foreach (var t in FootprintTiles(location, FootprintCellType.OccupiedUntargetable))
                yield return t;
        }

        public IEnumerable<CPos> FrozenUnderFogTiles(CPos location)
        {
            foreach (var t in FootprintTiles(location, FootprintCellType.Empty))
                yield return t;

            foreach (var t in Tiles(location))
                yield return t;
        }

        /// <summary>
        /// Occupieds the cells.
        /// </summary>
        /// <returns>The cells.</returns>
        /// <param name="info">Info.</param>
        /// <param name="topLeft">Top left.</param>
        /// <param name="subCell">Sub cell.</param>
        public IReadOnlyDictionary<CPos,SubCell> OccupiedCells(ActorInfo info,CPos topLeft,SubCell subCell = SubCell.Any)
        {
            var occupied = UnpathableTiles(topLeft).ToDictionary(c => c, c => SubCell.FullCell);
            return new ReadOnlyDictionary<CPos,SubCell>(occupied);
        }

        bool IOccupySpaceInfo.SharesCell { get { return false; } }

        public IEnumerable<CPos> UnpathableTiles(CPos location)
        {
            foreach (var t in FootprintTiles(location, FootprintCellType.Occupied))
                yield return t;

            foreach (var t in FootprintTiles(location, FootprintCellType.OccupiedUntargetable))
                yield return t;
        }

        public IEnumerable<CPos> PathableTiles(CPos location)
        {
            foreach (var t in FootprintTiles(location, FootprintCellType.Empty))
                yield return t;

            foreach (var t in FootprintTiles(location, FootprintCellType.OccupiedPassable))
                yield return t;
        }


        public WVec CenterOffset(World w)
        {
            var off = (w.Map.CenterOfCell(new CPos(Dimensions.X, Dimensions.Y)) - w.Map.CenterOfCell(new CPos(1, 1))) / 2;
            return (off - new WVec(0, 0, off.Z)) + LocalCenterOffset;
        }

        public virtual object Create(ActorInitializer init)
        {
            return new Building(init,this);
        }


        public IEnumerable<IRenderable> Render(WorldRenderer wr,World w,ActorInfo ai,WPos centerPosition){

            if (!RequiresBaseProvider)
                return SpriteRenderable.None;

            return w.ActorsWithTrait<BaseProvider>().SelectMany(a => a.Trait.RangeCircleRenderables(wr));
        }


        public virtual bool IsCloseEnoughToBase(World world,Player p,string buildingName,CPos topLeft)
        {

            var requiresBuildableArea = world.Map.Rules.Actors[buildingName].TraitInfoOrDefault<RequiresBuildableAreaInfo>();

            var mapBuildRadius = world.WorldActor.Trait<MapBuildRadius>();

            if (requiresBuildableArea == null || p.PlayerActor.Trait<DeveloperMode>().BuildAnywhere)
                return true;
            
            var adjacent = requiresBuildableArea.Adjacent;
            var buildingMaxBounds = Dimensions;

            var scanStart = world.Map.Clamp(topLeft - new CVec(adjacent, adjacent));
            var scanEnd = world.Map.Clamp(topLeft + buildingMaxBounds + new CVec(adjacent, adjacent));

            var nearnessCandidates = new List<CPos>();
            var bi = world.WorldActor.Trait<BuildingInfluence>();
            var allyBuildEnabled = mapBuildRadius.AllyBuildRadiusEnabled;


            for (var y = scanStart.Y; y < scanEnd.Y; y++)
            {
                for (var x = scanStart.X; x < scanEnd.X; x++)
                {
                    var pos = new CPos(x, y);

                    var buildingAtPos = bi.GetBuildingAt(pos);

                    if (buildingAtPos == null)
                    {
                        var unitsAtPos = world.ActorMap.GetActorsAt(pos).Where(a => a.IsInWorld
                            && (a.Owner == p || (allyBuildEnabled && a.Owner.Stances[p] == Stance.Ally))
                            && ActorGrantsValidArea(a, requiresBuildableArea));

                        if (unitsAtPos.Any())
                            nearnessCandidates.Add(pos);
                    }
                    else if (buildingAtPos.IsInWorld && ActorGrantsValidArea(buildingAtPos, requiresBuildableArea)
                        && (buildingAtPos.Owner == p || (allyBuildEnabled && buildingAtPos.Owner.Stances[p] == Stance.Ally)))
                        nearnessCandidates.Add(pos);
                }
            }
            var buildingTiles = Tiles(topLeft).ToList();
            return nearnessCandidates
                .Any(a => buildingTiles
                    .Any(b => Math.Abs(a.X - b.X) <= adjacent
                        && Math.Abs(a.Y - b.Y) <= adjacent));        
            
        }

        bool ActorGrantsValidArea(Actor a, RequiresBuildableAreaInfo rba)
        {
            return rba.AreaTypes.Overlaps(a.TraitsImplementing<GivesBuildableArea>()
                .SelectMany(gba => gba.AreaTypes));
        }

        public Actor FindBaseProvider(World world,Player p,CPos topLeft){

            var center = world.Map.CenterOfCell(topLeft) + CenterOffset(world);
            var mapBuildRadius = world.WorldActor.Trait<MapBuildRadius>();
            var allyBuildEnabled = mapBuildRadius.AllyBuildRadiusEnabled;

            if (!mapBuildRadius.BuildRadiusEnabled)
                return null;

            foreach(var bp in world.ActorsWithTrait<BaseProvider>()){

                var validOwner = bp.Actor.Owner == p || (allyBuildEnabled && bp.Actor.Owner.Stances[p] == Stance.Ally);
                if (!validOwner || !bp.Trait.Ready())
                    continue;


                var target = Target.FromPos(bp.Actor.CenterPosition);
                if (target.IsInRange(center, bp.Trait.Info.Range))
                    return bp.Actor;
                    
            }

            return null;
        }
    }



    public class Building:IOccupySpace,ISync,INotifyAddedToWorld,INotifyRemovedFromWorld,INotifyCreated,INotifySold,ITargetableCells
    {
        public readonly BuildingInfo Info;

        readonly Actor self;
        [Sync]
        readonly CPos topLeft;

        public bool BuildComplete { get; private set; }

        public CPos TopLeft { get { return topLeft; } }

        public WPos CenterPosition { get; private set; }

        public readonly bool SkipMakeAnimation;

        readonly BuildingInfluence influence;

        /// <summary>
        /// Shared activity lock:undeploy,sell,capture,etc.
        /// 共享活动锁定：取消部署，出售，捕获等
        /// </summary>
        [Sync]
        public bool Locked = true;

        Pair<CPos, SubCell>[] occupiedCells;
        Pair<CPos, SubCell>[] targetableCells;

        public Building(ActorInitializer init,BuildingInfo info)
        {
            self = init.Self;
            topLeft = init.Get<LocationInit, CPos>();

            Info = info;

            influence = self.World.WorldActor.Trait<BuildingInfluence>();


            occupiedCells = Info.UnpathableTiles(TopLeft).Select(c => Pair.New(c, SubCell.FullCell)).ToArray();

            targetableCells = Info.FootprintTiles(TopLeft, FootprintCellType.Occupied).Select(c => Pair.New(c, SubCell.FullCell)).ToArray();

            CenterPosition = init.World.Map.CenterOfCell(topLeft) + Info.CenterOffset(init.World);

            SkipMakeAnimation = init.Contains<SkipMakeAnimsInit>();
        }


        public IEnumerable<Pair<CPos,SubCell>> TargetableCells()
        {
            return targetableCells;
        }

        public Pair<CPos,SubCell>[] OccupiedCells() { return occupiedCells; }


        public bool Lock(){

            if (Locked)
                return false;
            Locked = true;
            return Locked;
        }

        public void UnLock(){
            Locked = false;
        }

        void INotifyCreated.Created(Actor self)
        {
            if (SkipMakeAnimation || !self.Info.HasTraitInfo<WithMakeAnimationInfo>())
                NotifyBuildingComplete(self);
            
        }

        public void NotifyBuildingComplete(Actor self){

            if (BuildComplete)
                return;

            BuildComplete = true;
            UnLock();
            foreach(var notify in self.TraitsImplementing<INotifyBuildComplete>()){
                notify.BuildingComplete(self);
            }

        }


        protected virtual void AddedToWorld(Actor self)
        {
            if (Info.RemoveSmudgesOnBuild)
                RemoveSmudges();

            self.World.AddToMaps(self, this);
            influence.AddInfluence(self,Info.Tiles(self.Location));

        }

        void INotifyAddedToWorld.AddedToWorld(Actor self){
            AddedToWorld(self);
        }

        void INotifyRemovedFromWorld.RemovedFromWorld(Actor self)
        {
            self.World.RemoveFromMaps(self, this);
            influence.RemoveInfluence(self,Info.Tiles(self.Location));
        }


        void INotifySold.Selling(Actor self)
        {

        }


        void INotifySold.Sold(Actor self)
        {

        }


        /// <summary>
        /// 移除印跡
        /// </summary>
        public void RemoveSmudges()
        {
            var smudgeLayers = self.World.WorldActor.TraitsImplementing<SmudgeLayer>();

            foreach(var smudgeLayer in smudgeLayers){
                foreach (var footprintTile in Info.Tiles(self.Location))
                    smudgeLayer.RemoveSmudge(footprintTile);
            }
        }
    }
}