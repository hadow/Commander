using System;
using EW.Traits;

namespace EW.Mods.Common.Traits
{

    public class CapturesInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new Captures(this); }
    }
    public class Captures
    {

        public Captures(CapturesInfo info)
        {

        }
    }
}