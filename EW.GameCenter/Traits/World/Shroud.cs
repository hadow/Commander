using System;
using System.Collections.Generic;

namespace EW.Traits
{
    public class ShroudInfo:ITraitInfo
    {
        public bool FogEnabled = true;

        public bool FogLocked = false;

        public bool ExploredMapEnabled = false;

        public bool ExploredMapLocked = false;

        public object Create(ActorInitializer init) { return new Shroud(init.Self); }
    }


    public class Shroud:ISync,INotifyCreated
    {
        readonly Actor self;
        readonly Map map;

        readonly CellLayer<short> visibleCount;
        readonly CellLayer<short> generatedShroudCount;
        readonly CellLayer<bool> explored;


        bool disabled;

        [Sync]
        public bool Disabled
        {
            get { return disabled; }
        }

        public int Hash { get; private set; }

        public Shroud(Actor self)
        {
            this.self = self;
            map = this.self.World.Map;

            visibleCount = new CellLayer<short>(map);
            generatedShroudCount = new CellLayer<short>(map);
            explored = new CellLayer<bool>(map);
        }
        void INotifyCreated.Created(Actor self)
        {

        }
    }
}