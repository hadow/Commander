using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class ParachutableInfo:ITraitInfo,Requires<IPositionableInfo>
    {

        public object Create(ActorInitializer init) { return new Parachutable(); }
    }
    public class Parachutable
    {
        public Parachutable()
        {
        }

        public bool IsInAir { get; private set; }

    }
}
