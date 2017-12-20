using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    class ChangesTerrainInfo:ITraitInfo
    {
        public object Create(ActorInitializer init) { return new ChangesTerrain(); }
    }

    class ChangesTerrain
    {

    }
}