using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{


    class CrateInfo : ITraitInfo,IPositionableInfo,Requires<RenderSpritesInfo>
    {
        public readonly int Lifetime = 0;

        public readonly HashSet<string> TerrainTypes = new HashSet<string>();

        public readonly string CrushClass = "crate";

        public object Create(ActorInitializer init)
        {
            return new Crate();
        }

        public IReadOnlyDictionary<CPos,SubCell> OccupiedCells(ActorInfo info,CPos location,SubCell subCell = SubCell.Any)
        {
            var occupied = new Dictionary<CPos, SubCell>() { { location, subCell } };
            return new ReadOnlyDictionary<CPos, SubCell>(occupied);
        }

        bool IOccupySpaceInfo.SharesCell { get { return false; } }

        public bool CanEnterCell(World world,Actor self,CPos cell,Actor ignoreActor = null,bool checkTransientActors = true)
        {
            return false;
        }



    }
    class Crate
    {
    }
}