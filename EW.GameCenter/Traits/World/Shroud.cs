using System;
using System.Collections.Generic;

namespace EW.Traits
{
    public class ShroudInfo:ITraitInfo
    {
        public bool FogEnabled = true;

        public bool FogLocked = false;

        public bool ExploredMapEnabled = false;

        public object Create(ActorInitializer init) { return new Shroud(); }
    }
    public class Shroud:ISync,INotifyCreated
    {

        void INotifyCreated.Created(Actor self)
        {

        }
    }
}