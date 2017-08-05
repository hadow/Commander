using System;

namespace EW.Traits
{


    public class DebugPauseStateInfo : ITraitInfo
    {
        public object Create(ActorInitializer init) { return new DebugPauseState(init.World); }
    }

    public class DebugPauseState:ISync
    {
        readonly World world;

        [Sync]
        public bool Paused { get { return world.Paused; } }

       public DebugPauseState(World world)
        {
            this.world = world;
        }
    }
}