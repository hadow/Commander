using System;
using EW.Traits;
namespace EW.Mods.Common.Traits
{
    public class FreeeActorInfo : ITraitInfo
    {
        public virtual object Create(ActorInitializer init) { return new FreeActor(); }
    }
    class FreeActor
    {
    }
}