using System;
using System.Collections.Generic;
using System.Linq;
using EW.Primitives;
using EW.Traits;
using EW.Activities;
using EW.Mods.Common.Activities;
namespace EW.Mods.Common.Traits
{
    [Flags]
    public enum CellConditions
    {
        None = 0,
        TransientActors,//短暂的，路过的
        BlockedByMovers,//被移动者阻挡
        All = TransientActors | BlockedByMovers
    }


    public static class CustomMovementLayerType
    {
        public const byte Tunnel = 1;           //遂道
        public const byte Subterranean = 2;
        public const byte Jumpjet = 3;          //垂直起降喷气机
        public const byte ElevatedBridge = 4;   //高架桥
    }
    public static class CellConditionsExts
    {
        public static bool HasCellCondition(this CellConditions c,CellConditions cellCondition)
        {
            //PERF:Enum.HasFlag is slower and requires allocations.
            return (c & cellCondition) == cellCondition;
        }
    }
    /// <summary>
    /// Unit is able to move
    /// 单位能够移动
    /// </summary>
    public class MobileInfo:ConditionalTraitInfo,IMoveInfo,IPositionableInfo,IFacingInfo,
        UsesInit<FacingInit>, UsesInit<LocationInit>, UsesInit<SubCellInit>
    {

        /// <summary>
        /// 地形信息
        /// </summary>
        public class TerrainInfo
        {
            /// <summary>
            /// 不能通行
            /// </summary>
            public static readonly TerrainInfo Impassable = new TerrainInfo();

            public readonly int Cost;

            public readonly int Speed;

            public TerrainInfo()
            {
                Cost = int.MaxValue;
                Speed = 0;
            }

            public TerrainInfo(int speed,int cost)
            {
                Speed = speed;
                Cost = cost;
            }
        }
        /// <summary>
        /// This struct allows us to cache the terrain info for the tileset used by the world.
        /// This allows us to speed up some performance-sensitive pathfinding calculations.
        /// 这个结构允许我们缓存世界使用的tileset的地形信息。这使我们能够加快一些性能敏感的寻路计算。
        /// </summary>
        public struct WorldMovementInfo
        {
            internal readonly World World;
            internal readonly TerrainInfo[] TerrainInfos;

            internal WorldMovementInfo(World world,MobileInfo info)
            {
                World = world;
                TerrainInfos = info.TilesetTerrainInfo[world.Map.Rules.TileSet];

            }
        }


        /// <summary>
        /// Allow multiple units in one cell.
        /// 在一个单元格中允许多个单元。
        /// </summary>
        public readonly bool SharesCell = false;

        /// <summary>
        /// Speed at which the actor turns.
        /// </summary>
        public readonly int TurnSpeed = 255;

        public readonly int InitialFacing = 0;

        public readonly int WaitSpread = 2;

        public readonly int WaitAverage = 5;

        public readonly int Speed = 1;

        public readonly bool OnRails = false;

        public readonly bool MoveIntoShroud = true; //Can the actor be ordered to move in to shroud?

        [VoiceReference]
        public readonly string Voice = "Action";

        /// <summary>
        /// The condition to grant to self while inside a tunnel.
        /// </summary>
        public readonly string TunnelCondition = null;

        /// <summary>
        /// Can this unit move underground?
        /// </summary>
        public readonly bool Subterranean = false;

        /// <summary>
        /// The condition to grant to self while underground.
        /// </summary>
        public readonly string SubterraneanCondition = null;

        /// <summary>
        /// Pathfinding cost for submerging or reemerging.
        /// </summary>
        public readonly int SubterraneanTransitionCost = 0;

        /// <summary>
        /// The terrain types that this actor can transition on.Leave empty to allow any.
        /// </summary>
        public readonly HashSet<string> SubterraneanTransitionTerrainTypes = new HashSet<string>();

        /// <summary>
        /// Can this actor transition on slopes?
        /// </summary>
        public readonly bool SubterraneanTransitionOnRamps = false;

        public readonly WDist SubterraneanTransitionDepth = new WDist(-1024);

        /// <summary>
        /// Dig animation image to play when transitioning.
        /// 在过渡时挖掘动画图像进行播放
        /// </summary>
        public readonly string SubterraneanTransitionImage = null;

        [SequenceReference("SubterraneanTransitionImage")]
        public readonly string SubterraneanTransitionSequence = null;

        [PaletteReference]
        public readonly string SubterraneanTransitionPalette = "effect";

        public readonly string SubterraneanTransitionSound = null;

        /// <summary>
        /// The condition to grant to self while flying.
        /// </summary>
        [GrantedConditionReference]
        public readonly string JumpjetCondition = null;

        /// <summary>
        /// Pathfinding cost for taking off or landing
        /// 寻找起飞或着陆的成本
        /// </summary>
        public readonly int JumpjetTransitionCost = 0;

        /// <summary>
        /// Can this unit fly over obstacles?
        /// 这个单位可以飞越障碍吗？
        /// </summary>
        public readonly bool Jumpjet = false;

        /// <summary>
        /// The terrain types that this actor can transition on.Leave empty to allow any.
        /// </summary>
        public readonly HashSet<string> JumpjetTransitionTerrainTypes = new HashSet<string>();

        /// <summary>
        /// Can this actor transition on slopes?
        /// </summary>
        public readonly bool JumpjetTransitionOnRamps = true;
        /// <summary>
        /// Set Water:0 for ground units and lower the value on rough terrain;
        /// 设置水：地面单位为0，降低粗糙地形的值
        /// </summary>
        [FieldLoader.LoadUsing("LoadSpeeds",true)]
        public readonly Dictionary<string, TerrainInfo> TerrainSpeeds;

        /// <summary>
        /// 
        /// </summary>
        public readonly HashSet<string> Crushes = new HashSet<string>();

        public readonly Cache<TileSet, TerrainInfo[]> TilesetTerrainInfo;
        public readonly Cache<TileSet, int> TilesetMovementClass;

        public MobileInfo()
        {
            TilesetTerrainInfo = new Cache<TileSet, TerrainInfo[]>(LoadTilesetSpeeds);
            TilesetMovementClass = new Cache<TileSet, int>(CalculateTilesetMovementClass);
        }
        
        
        public IReadOnlyDictionary<CPos,SubCell> OccupiedCells(ActorInfo info,CPos location,SubCell subCell = SubCell.Any)
        {
            return new ReadOnlyDictionary<CPos, SubCell>(new Dictionary<CPos, SubCell>() { { location, subCell } });
        }

        bool IOccupySpaceInfo.SharesCell { get { return SharesCell; } }

        public int GetInitialFacing() { return InitialFacing; }

        public override  object Create(ActorInitializer init)
        {
            return new Mobile(init, this);
        }

        public WorldMovementInfo GetWorldMovementInfo(World world)
        {
            return new WorldMovementInfo(world, this);
        }

        /// <summary>
        /// 判断是否可进入单元格
        /// </summary>
        /// <param name="world"></param>
        /// <param name="self"></param>
        /// <param name="cell"></param>
        /// <param name="ignoreActor"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        public bool CanEnterCell(World world,Actor self,CPos cell,Actor ignoreActor = null,bool checkTransientActors = true)
        {
            if (MovementCostForCell(world, cell) == int.MaxValue)
                return false;

            var check = checkTransientActors ? CellConditions.All : CellConditions.BlockedByMovers;
            return CanMoveFreelyInto(world, self, cell, ignoreActor, check);
        }

        /// <summary>
        /// Determines whether the actor is blocked by other Actors.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="self"></param>
        /// <param name="cell"></param>
        /// <param name="ignoreActor"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        public bool CanMoveFreelyInto(World world,Actor self,CPos cell, Actor ignoreActor,CellConditions check)
        {
            if (!check.HasCellCondition(CellConditions.TransientActors))
                return true;

            if (SharesCell && world.ActorMap.HasFreeSubCell(cell))
                return true;

            foreach (var otherActor in world.ActorMap.GetActorsAt(cell))
                if (IsBlockedBy(self, otherActor, ignoreActor, check))
                    return false;

            return true;
        }

        /// <summary>
        /// 是否阻挡
        /// </summary>
        /// <param name="self"></param>
        /// <param name="otherActor"></param>
        /// <param name="ignoreActor"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        bool IsBlockedBy(Actor self,Actor otherActor,Actor ignoreActor,CellConditions check)
        {
            // We are not blocked by the actor we are ignoring;
            if (otherActor == ignoreActor)
                return false;

            if (self == null)
                return true;
            if (!check.HasCellCondition(CellConditions.BlockedByMovers) &&
                self.Owner.Stances[otherActor.Owner] == Stance.Ally &&
                IsMovingInMyDirection(self,otherActor))
                return false;

            var temporaryBlocker = otherActor.TraitOrDefault<ITemporaryBlocker>();
            if (temporaryBlocker != null && temporaryBlocker.CanRemoveBlockage(otherActor, self))
                return false;

            //If we cannot crush the other actor in our way,we are blocked.
            if (Crushes == null || Crushes.Count == 0)
                return true;

            var crushables = otherActor.TraitsImplementing<ICrushable>();
            var lacksCrushability = true;

            foreach(var crushable in crushables)
            {
                lacksCrushability = false;
                if (!crushable.CrushableBy(otherActor, self, Crushes))
                    return true;
            }

            if (lacksCrushability)
                return true;
            //We are not blocked by the other actor.
            return false;
        }

        static bool IsMovingInMyDirection(Actor self,Actor other)
        {
            return false;
        }

        /// <summary>
        /// 移动至某一单元格的成本
        /// </summary>
        /// <param name="worldMovementInfo"></param>
        /// <param name="self"></param>
        /// <param name="cell"></param>
        /// <param name="ignoreActor"></param>
        /// <param name="check"></param>
        /// <returns></returns>
        public int MovementCostToEnterCell(WorldMovementInfo worldMovementInfo,Actor self,CPos cell,Actor ignoreActor = null,CellConditions check = CellConditions.All)
        {
            var cost = MovementCostForCell(worldMovementInfo.World.Map, worldMovementInfo.TerrainInfos, cell);
            if (cost == int.MaxValue || !CanMoveFreelyInto(worldMovementInfo.World, self, cell, ignoreActor, check))
                return int.MaxValue;
            return cost;
        }
        /// <summary>
        /// 移动的代价
        /// </summary>
        /// <param name="world"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        public int MovementCostForCell(World world,CPos cell)
        {
            return MovementCostForCell(world.Map, TilesetTerrainInfo[world.Map.Rules.TileSet], cell);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="map"></param>
        /// <param name="terrainInfos"></param>
        /// <param name="cell"></param>
        /// <returns></returns>
        int MovementCostForCell(Map map,TerrainInfo[] terrainInfos,CPos cell)
        {
            if (map.Contains(cell))
                return int.MaxValue;

            var index = map.GetTerrainIndex(cell);
            if (index == byte.MaxValue)
                return int.MaxValue;
            return terrainInfos[index].Cost;
        }

        static object LoadSpeeds(MiniYaml y)
        {
            var ret = new Dictionary<string, TerrainInfo>();

            foreach(var t in y.ToDictionary()["TerrainSpeeds"].Nodes)
            {
                var speed = FieldLoader.GetValue<int>("speed", t.Value.Value);
                var nodesDict = t.Value.ToDictionary();
                var cost = nodesDict.ContainsKey("PathingCost") ? FieldLoader.GetValue<int>("cost", nodesDict["PathingCost"].Value) : 1000 / speed;
                ret.Add(t.Key, new TerrainInfo(speed, cost));
            }

            return ret;
        }

        /// <summary>
        /// 移动单位在不同地形下的速度
        /// </summary>
        /// <param name="tileSet"></param>
        /// <returns></returns>
        TerrainInfo[] LoadTilesetSpeeds(TileSet tileSet)
        {
            var info = new TerrainInfo[tileSet.TerrainInfo.Length];
            for (var i = 0; i < info.Length; i++)
                info[i] = TerrainInfo.Impassable;

            foreach(var kvp in TerrainSpeeds)
            {
                byte index;
                if (tileSet.TryGetTerrainIndex(kvp.Key, out index))
                    info[index] = kvp.Value;
            }
            return info;

        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tileset"></param>
        /// <returns></returns>
        public int CalculateTilesetMovementClass(TileSet tileset)
        {
            //Collect our ability to cross *all* terraintypes,in a bitvector
            return TilesetTerrainInfo[tileset].Select(ti => ti.Cost < int.MaxValue).ToBits();
        }

        public int GetMovementClass(TileSet tileset)
        {
            return TilesetMovementClass[tileset];
        }

        
    }

    public class Mobile:ConditionalTrait<MobileInfo>,
        INotifyCreated,
        IPositionable,
        INotifyAddToWorld,
        INotifyRemovedFromWorld,
        IMove,
        IFacing,
        ISync,
        IActorPreviewInitModifier,
        IDeathActorInitModifier,
        INotifyBlockingMove
    {
        const int AverageTicksBeforePathing = 5;
        const int SpreadTicksBeforePathing = 5;
        internal int TicksBeforePathing = 0;

        ConditionManager conditionManager;
        int tunnelToken = ConditionManager.InvalidConditionToken;
        int subterraneanToken = ConditionManager.InvalidConditionToken;
        int jumpjetToken = ConditionManager.InvalidConditionToken;

        readonly Actor self;
        readonly Lazy<IEnumerable<int>> speedModifiers;

        CPos fromCell, toCell;

        public SubCell FromSubCell, ToSubCell;

        [Sync]
        public int PathHash;

        [Sync]
        public CPos ToCell { get { return toCell; } }

        [Sync]
        public CPos FromCell { get { return fromCell; } }

        [Sync]
        public WPos CenterPosition { get; private set; }

        public CPos TopLeft { get { return ToCell; } }

        public int TurnSpeed { get { return Info.TurnSpeed; } }


        public bool IsMoving { get; set; }

        public bool IsMovingVertically { get { return false; } }
        int facing;
        [Sync]
        public int Facing
        {
            get { return facing; }
            set { facing = value; }
        }


        public Mobile(ActorInitializer init, MobileInfo info): base(info)
        {
            self = init.Self;

            speedModifiers = Exts.Lazy(() => self.TraitsImplementing<ISpeedModifier>().ToArray().Select(x => x.GetSpeedModifier()));

            ToSubCell = FromSubCell = info.SharesCell ? init.World.Map.Grid.DefaultSubCell : SubCell.FullCell;

            if (init.Contains<SubCellInit>())
                FromSubCell = ToSubCell = init.Get<SubCellInit, SubCell>();

            if (init.Contains<LocationInit>())
            {
                fromCell = toCell = init.Get<LocationInit, CPos>();
                SetVisualPosition(self, init.World.Map.CenterOfSubCell(FromCell, FromSubCell));
            }

            Facing = init.Contains<FacingInit>() ? init.Get<FacingInit,int>() : info.InitialFacing;

            //Sets the visual position to WPos accuracy
            //Use LocationInit if you want to insert the actor into the ActorMap!

            if (init.Contains<CenterPositionInit>())
                SetVisualPosition(self, init.Get<CenterPositionInit, WPos>());
        }

        protected override void Created(Actor self)
        {

            conditionManager = self.TraitOrDefault<ConditionManager>();
            base.Created(self);
        }

        public IEnumerable<Pair<CPos,SubCell>> OccupiedCells()
        {
            if (FromCell == ToCell)
                return new[] { Pair.New(FromCell, FromSubCell) };
            if (CanEnterCell(ToCell))
                return new[] { Pair.New(ToCell, ToSubCell) };

            return new[] { Pair.New(FromCell, FromSubCell), Pair.New(ToCell, ToSubCell) };
        }

        

        public bool CanEnterCell(CPos cell,Actor ignoreActor = null,bool checkTransientActors = true)
        {
            return Info.CanEnterCell(self.World, self, cell,ignoreActor, checkTransientActors);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="location"></param>
        /// <param name="subCell"></param>
        /// <returns></returns>
        public bool IsLeavingCell(CPos location,SubCell subCell = SubCell.Any)
        {
            return ToCell != location && fromCell == location && (subCell == SubCell.Any || FromSubCell == subCell || subCell == SubCell.FullCell || FromSubCell == SubCell.FullCell);
        }

        /// <summary>
        /// Sets the location (fromCell,toCell,FromSubCell,ToSubCell) and visual position(CenterPosition)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="cell"></param>
        /// <param name="subCell"></param>
        public void SetPosition(Actor self,CPos cell,SubCell subCell = SubCell.Any)
        {
            subCell = GetValidSubCell(subCell);
            SetLocation(cell, subCell, cell, subCell);
            SetVisualPosition(self, self.World.Map.CenterOfSubCell(cell, subCell));
            FinishedMoving(self);
        }

        public void SetPosition(Actor self, WPos pos)
        {
            var cell = self.World.Map.CellContaining(pos);
            SetLocation(cell, FromSubCell, cell, FromSubCell);
            SetVisualPosition(self, self.World.Map.CenterOfSubCell(cell, FromSubCell) + new WVec(0, 0, self.World.Map.DistanceAboveTerrain(pos).Length));
            FinishedMoving(self);
        }

        public void FinishedMoving(Actor self)
        {
            if (!self.IsAtGroundLevel())
                return;
            var actors = self.World.ActorMap.GetActorsAt(toCell).Where(a => a != self).ToList();
            if (!AnyCrushables(actors))
                return;

            var notifiers = actors.SelectMany(a => a.TraitsImplementing<INotifyCrushed>().Select(t => new TraitPair<INotifyCrushed>(a, t)));
            foreach (var notifyCrushed in notifiers)
                notifyCrushed.Trait.OnCrush(notifyCrushed.Actor, self, Info.Crushes);
        }

        bool AnyCrushables(List<Actor> actors)
        {
            return true;
        }

        

        /// <summary>
        /// Sets only the visual position(CenterPosition)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="pos"></param>
        public void SetVisualPosition(Actor self,WPos pos)
        {
            CenterPosition = pos;
            self.World.UpdateMaps(self, this);
        }

        public SubCell GetValidSubCell(SubCell preferred = SubCell.Any)
        {
            if (preferred == SubCell.Any)
                preferred = FromSubCell;

            if (Info.SharesCell)
            {
                if (preferred <= SubCell.FullCell)
                    return self.World.Map.Grid.DefaultSubCell;
            }
            else
            {
                if (preferred != SubCell.FullCell)
                    return SubCell.FullCell;
            }
            return preferred;
        }

        /// <summary>
        /// Sets only the location ( fromCell,toCell,FromSubCell,ToSubCell)
        /// </summary>
        /// <param name="from"></param>
        /// <param name="fromSub"></param>
        /// <param name="to"></param>
        /// <param name="toSub"></param>
        public void SetLocation(CPos from,SubCell fromSub,CPos to,SubCell toSub)
        {
            if (FromCell == from && ToCell == to && FromSubCell == fromSub && ToSubCell == toSub)
                return;
            RemoveInfluence();
            fromCell = from;
            toCell = to;
            FromSubCell = fromSub;
            ToSubCell = toSub;
            AddInfluence();
        }

        public void RemoveInfluence()
        {
            if (self.IsInWorld)
                self.World.ActorMap.RemoveInfluence(self, this);
        }

        public void AddInfluence()
        {
            if (self.IsInWorld)
                self.World.ActorMap.AddInfluence(self, this);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        /// <param name="nudger">推动者</param>
        /// <param name="force"></param>
        public void Nudge(Actor self,Actor nudger,bool force)
        {

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="self"></param>
        public void AddedToWorld(Actor self)
        {
            self.World.AddToMaps(self, this);
        }

        public void RemovedFromWorld(Actor self)
        {
            self.World.RemoveFromMaps(self, this);
        }


        //public Activity ScriptedMove(CPos cell) { return new Move(self, cell); }

        #region IMove interface

        

        /// <summary>
        /// 附近可移动的单元格
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public CPos NearestMoveableCell(CPos target)
        {
            throw new NotImplementedException();
        }

        public Activity MoveTo(CPos cell,int nearEnough) { return new Move(self, cell, WDist.FromCells(nearEnough)); }

        public Activity MoveTo(CPos cell,Actor ignoredActor) { return new Move(self, cell, ignoredActor); }

        public Activity MoveWithinRange(Target target,WDist range) { return new MoveWithinRange(self, target, WDist.Zero, range); }

        public Activity MoveWithinRange(Target target,WDist minRange,WDist maxRange) { return new MoveWithinRange(self, target, minRange, maxRange); }

        public Activity MoveFollow(Actor self,Target target,WDist minRange,WDist maxRange) { return new Follow(self, target, minRange, maxRange); }

        public Activity MoveTo(Func<List<CPos>> pathFunc) { return new Move(self, pathFunc); }

        public Activity MoveToTarget(Actor self,Target target)
        {
            throw new NotImplementedException();
        }

        public Activity MoveIntoTarget(Actor self,Target target)
        {
            throw new NotImplementedException();
        }

        public bool CanEnterTargetNow(Actor self,Target target)
        {
            throw new NotImplementedException();
        }

        public Activity MoveIntoWorld(Actor self,CPos cell,SubCell subCell = SubCell.Any)
        {
            throw new NotImplementedException();
        }

        public Activity VisualMove(Actor self,WPos fromPos,WPos toPos)
        {
            throw new NotImplementedException();
        }
        public Activity VisualMove(Actor self,WPos fromPos,WPos toPos,CPos cell)
        {
            throw new NotImplementedException();
        }
        
        
        #endregion

        void IActorPreviewInitModifier.ModifyActorPreviewInit(Actor self,TypeDictionary inits)
        {

        }

        void IDeathActorInitModifier.ModifyDeathActorInit(Actor self,TypeDictionary inits)
        {

        }

        public void OnNotifyBlockingMove(Actor self,Actor blocking)
        {

        }

        public SubCell GetAvailableSubCell(CPos a,SubCell preferredSubCell = SubCell.Any,Actor ignoreActor = null,bool checkTransientActors = true)
        {
            throw new NotImplementedException();
        }
    }
}