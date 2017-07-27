using System;

using EW.Traits;
namespace EW.Mods.Common.Traits
{


    public class GivesBuildableAreaInfo : TraitInfo<GivesBuildableArea>
    {

    }
    public class GivesBuildableArea
    {

    }

    public class BuildingInfo : ITraitInfo
    {
        public object Create(ActorInitializer init)
        {
            return new Building();
        }
    }
    public class Building
    {

        public bool BuildComplete { get; private set; }
    }
}