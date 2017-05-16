using System;
using System.Collections.Generic;


namespace EW.Mods.Common.Traits
{
    public class CapturableInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new Capturable(); }
    }
    public class Capturable
    {
    }
}