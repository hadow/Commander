using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class PlaceBuildingInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new PlaceBuilding(this); }
    }
    public class PlaceBuilding
    {
        readonly PlaceBuildingInfo info;
        public PlaceBuilding(PlaceBuildingInfo info)
        {
            this.info = info;
        }
    }
}