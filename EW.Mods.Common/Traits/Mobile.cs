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
        BlockedByMovers,
        All = TransientActors | BlockedByMovers
    }

    public static class CellConditionsExts
    {
        public static bool HasCellCondition(this CellConditions c,CellConditions cellCondition)
        {
            return (c & cellCondition) == cellCondition;
        }
    }
    /// <summary>
    /// Unit is able to move
    /// </summary>
    public class MobileInfo:UpgradableTraitInfo,IMoveInfo,IPositionableInfo,IOccupySapceInfo,IFacingInfo,
        UsesInit<FacingInit>, UsesInit<LocationInit>, UsesInit<SubCellInit>
    {
        /// <summary>
        /// Allow multiple units in one cell.
        /// </summary>
        public readonly bool SharesCell = false;
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

        [FieldLoader.LoadUsing("LoadSpeeds",true)]
        public readonly Dictionary<string, TerrainInfo> TerrainSpeeds;


        public readonly Cache<TileSet, TerrainInfo[]> TilesetTerrainInfo;
        public readonly Cache<TileSet, int> TilesetMovementClass;

        public MobileInfo()
        {
            TilesetTerrainInfo = new Cache<TileSet, TerrainInfo[]>(LoadTilesetSpeeds);
            TilesetMovementClass = new Cache<TileSet, int>(CalculateTilesetMovementClass);
        }
        
        public readonly int InitialFacing = 0;
        public IReadOnlyDictionary<CPos,SubCell> OccupiedCells(ActorInfo info,CPos location,SubCell subCell = SubCell.Any)
        {
            return new ReadOnlyDictionary<CPos, SubCell>(new Dictionary<CPos, SubCell>() { { location, subCell } });
        }

        bool IOccupySapceInfo.SharesCell { get { return SharesCell; } }

        public int GetInitialFacing() { return InitialFacing; }

        public override  object Create(ActorInitializer init)
        {
            return new Mobile(init, this);
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
        public bool CanEnterCell(World world,Actor self,CPos cell,Actor ignoreActor = null,CellConditions check = CellConditions.All)
        {
            if (MovementCostForCell(world, cell) == int.MaxValue)
                return false;
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
            //We are not blocked by the other actor.
            return false;
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
            return TilesetTerrainInfo[tileset].Select(ti => ti.Cost < int.MaxValue).ToBits();
        }
    }

    public class Mobile:UpgradableTrait<MobileInfo>,IPositionable,INotifyAddToWorld,INotifyRemovedFromWorld,IMove
    {
        readonly Actor self;

        CPos fromCell, toCell;

        public SubCell FromSubCell, ToSubCell;

        [Sync]
        public CPos ToCell { get { return toCell; } }

        [Sync]
        public CPos FromCell { get { return fromCell; } }

        [Sync]
        public WPos CenterPosition { get; private set; }

        public CPos TopLeft { get { return ToCell; } }

        public Mobile(ActorInitializer init, MobileInfo info): base(info)
        {
            self = init.Self;
        }

        public IEnumerable<Pair<CPos,SubCell>> OccupiedCells()
        {
            if (FromCell == ToCell)
                return new[] { Pair.New(FromCell, FromSubCell) };
            if (CanEnterCell(ToCell))
                return new[] { Pair.New(ToCell, ToSubCell) };

            return new[] { Pair.New(FromCell, FromSubCell), Pair.New(ToCell, ToSubCell) };
        }

        public Activity ScriptedMove(CPos cell) { return new Move(self, cell); }

        public bool CanEnterCell(CPos cell,Actor ignoreActor = null,bool checkTransientActors = true)
        {
            return Info.CanEnterCell(self.World, self, cell,ignoreActor, checkTransientActors ? CellConditions.All : CellConditions.BlockedByMovers);
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

        }

        public void SetPosition(Actor self,WPos pos)
        {

        }

        /// <summary>
        /// Sets only the visual position(CenterPosition)
        /// </summary>
        /// <param name="self"></param>
        /// <param name="pos"></param>
        public void SetVisualPosition(Actor self,WPos pos)
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
    }
}