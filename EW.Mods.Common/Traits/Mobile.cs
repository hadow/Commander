using System.Collections.Generic;
using EW.Primitives;
using EW.Traits;
using EW.Activities;
using EW.Mods.Common.Activities;
namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// Unit is able to move
    /// </summary>
    public class MobileInfo:UpgradableTraitInfo,IMoveInfo,IPositionableInfo,IOccupySapceInfo,IFacingInfo,
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
    }

    public class Mobile:UpgradableTrait<MobileInfo>
    {
        readonly Actor self;
        CPos fromCell, toCell;

        public CPos ToCell { get { return toCell; } }

        public CPos FromCell { get { return fromCell; } }
        public Mobile(ActorInitializer init, MobileInfo info): base(info)
        {

        }

        public Activity ScriptedMove(CPos cell) { return new Move(self, cell); }
    }
}