using System;
using System.Collections.Generic;
using EW.Traits;
namespace EW.Mods.Common.Traits
{

    public abstract class AffectsShroudInfo : ITraitInfo
    {
        public abstract object Create(ActorInitializer init);
    }

    public abstract class AffectsShroud
    {

        public AffectsShroud(Actor self,AffectsShroudInfo info) { }
    }
}