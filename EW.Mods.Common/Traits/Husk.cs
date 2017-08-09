using System;
using System.Collections.Generic;

using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class HuskInfo : ITraitInfo,IOccupySapceInfo
    {

        public object Create(ActorInitializer init) { return new Husk(); }
    }
    public class Husk
    {
    }
}