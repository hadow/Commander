using System;

namespace EW.Traits
{


    public class DebugPauseStateInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new DebugPauseState(); }
    }

    public class DebugPauseState
    {
    }
}