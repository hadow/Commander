using System;
using System.Collections.Generic;
using EW.Traits;

namespace EW.Mods.Common.Traits
{
    public class RevealsShroudInfo : AffectsShroudInfo
    {
        public override object Create(ActorInitializer init)
        {
            return new RevealsShroud(init.Self, this);
        }
    }
    public class RevealsShroud:AffectsShroud
    {
        public RevealsShroud(Actor self,RevealsShroudInfo info) : base(self, info) { }
    }
}