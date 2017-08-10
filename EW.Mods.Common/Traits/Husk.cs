using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class HuskInfo : ITraitInfo,IOccupySpaceInfo
    {
        public IReadOnlyDictionary<CPos,SubCell> OccupiedCells(ActorInfo info,CPos location,SubCell subCell = SubCell.Any)
        {
            var occupied = new Dictionary<CPos, SubCell>() { { location, SubCell.FullCell } };
            return new ReadOnlyDictionary<CPos, SubCell>(occupied);
        }

        bool IOccupySpaceInfo.SharesCell { get { return false; } }

        public object Create(ActorInitializer init) { return new Husk(); }
    }
    public class Husk
    {
    }
}