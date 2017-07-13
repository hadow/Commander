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

        public readonly bool SharesCell = false;
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

        public bool CanEnterCell(World world,Actor self,CPos cell,Actor ignoreActor = null,CellConditions check = CellConditions.All)
        {
            if (MovementCostForCell(world, cell) == int.MaxValue)
                return false;

        }

        public bool CanMoveFreelyInto(World world,Actor self,CPos cell, Actor ignoreActor,CellConditions check)
        {
            if (!check.HasCellCondition(CellConditions.TransientActors))
                return true;

        }

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

    public class Mobile:UpgradableTrait<MobileInfo>,IPositionable,INotifyAddToWorld,INotifyRemovedFromWorld
    {
        readonly Actor self;
        CPos fromCell, toCell;

        public CPos ToCell { get { return toCell; } }

        public CPos FromCell { get { return fromCell; } }
        public Mobile(ActorInitializer init, MobileInfo info): base(info)
        {
            self = init.Self;
        }

        public Activity ScriptedMove(CPos cell) { return new Move(self, cell); }

        public bool CanEnterCell(CPos cell,Actor ignoreActor = null,bool checkTransientActors = true)
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