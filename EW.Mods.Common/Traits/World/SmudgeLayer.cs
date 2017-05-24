using System;

namespace EW.Mods.Common.Traits
{

    public class SmudgeLayerInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new SmudgeLayer(); }
    }

    public class SmudgeLayer
    {
    }
}