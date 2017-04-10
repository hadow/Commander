using System;

namespace EW
{
    /// <summary>
    /// 
    /// </summary>
    public struct MapPos:IEquatable<MapPos>
    {

        public readonly int U, V;


        public bool Equals(MapPos mPos)
        {
            return mPos.U == this.U && mPos.V == this.V;
        }

        public CellPos ToCPos(Map map)
        {
            return ToCPos(map.Grid.Type);
        }

        public CellPos ToCPos(MapGridT gridT)
        {
            //TODO
            return default(CellPos);
        }

    }
}