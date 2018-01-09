using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    /// <summary>
    /// Renders the cargo loaded into the unit.
    /// </summary>
    public class WithCargoInfo : ITraitInfo, Requires<CargoInfo>, Requires<BodyOrientationInfo>
    {
        public readonly WVec[] LocalOffset = { WVec.Zero };

        public readonly HashSet<string> DisplayTypes = new HashSet<string>();
    
        public object Create(ActorInitializer init) { return new WithCargo(); }
    }

    public class WithCargo
    {
    }
}