using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class FreeActorInfo : ITraitInfo
    {
        public virtual object Create(ActorInitializer init) { return new FreeActor(); }
    }
    class FreeActor
    {
    }
}