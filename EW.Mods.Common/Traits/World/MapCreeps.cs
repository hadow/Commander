using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class MapCreepsInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new MapCreeps(); }
    }


    public class MapCreeps
    {
    }
}