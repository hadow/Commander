using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public class WithCargoInfo : ITraitInfo, Requires<CargoInfo>, Requires<BodyOrientationInfo>
    {
        public object Create(ActorInitializer init) { return new WithCargo(); }
    }

    public class WithCargo
    {
    }
}