using System;


namespace EW.Mods.Common.Traits
{

    /// <summary>
    /// 
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
    }

    public class Mobile:UpgradableTrait<>
    {
    }
}